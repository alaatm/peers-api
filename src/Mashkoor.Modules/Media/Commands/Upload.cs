using System.Reflection;
using Mashkoor.Core.AzureServices.Storage;
using Mashkoor.Core.Background;
using Mashkoor.Core.Cqrs.Pipeline;
using Mashkoor.Modules.Customers.Domain;
using Mashkoor.Modules.Media.Domain;
using Mashkoor.Modules.Media.Processing;

namespace Mashkoor.Modules.Media.Commands;

public static class Upload
{
    /// <summary>
    /// The command.
    /// </summary>
    /// <param name="TargetId">The target id for the media upload.</param>
    /// <param name="Metadata">A dictionary of metadata for the media files.</param>
    /// <param name="Files">The media files to upload.</param>
    [Authorize(Roles = Roles.Customer)]
    public sealed record Command(
        int? TargetId,
        Dictionary<string, Command.FileMetadata> Metadata,
        Core.Http.FormFile[] Files) : ICommand, IValidatable
    {
        public static async ValueTask<Command?> BindAsync([NotNull] HttpContext context, ParameterInfo _)
        {
            var data = await context.Request.BindFormDataAsync<Command>();
            var model = data.Data with { Files = data.Files };

            return model;
        }

        /// <summary>
        /// Represents metadata information for a file, including its media type, description, and optional video
        /// thumbnail file name.
        /// </summary>
        /// <param name="Type">The type of media content.</param>
        /// <param name="Description">An optional description of the media.</param>
        public sealed record FileMetadata(
            MediaType Type,
            string? Description);
    }

    public readonly record struct BatchIdObj(Guid BatchId);

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(p => p.TargetId)
                .GreaterThan(0)
                .When(p => p.TargetId.HasValue);

            RuleFor(p => p.Metadata)
                .NotNull()
                .NotEmpty();

            RuleFor(p => p.Metadata)
                .Must(p => p != null && p.All(kvp => kvp.Value is not null))
                .WithMessage("Metadata cannot contain null values.");

            RuleForEach(p => p.Metadata)
                .ChildRules(c => c.RuleFor(p => p.Value!.Type).IsInEnum())
                .When(p => p.Metadata != null && p.Metadata.All(kvp => kvp.Value is not null));

            RuleFor(p => p.Files)
                .NotNull()
                .NotEmpty();

            RuleFor(p => p.Files)
                .Must(p => p != null && p.All(f => f is not null))
                .WithMessage("'Files' cannot contain null entries.");

            RuleForEach(p => p.Files)
                .Must((cmd, file) =>
                    !string.IsNullOrWhiteSpace(file.Name) &&
                    cmd.Metadata is not null &&
                    cmd.Metadata.ContainsKey(file.Name))
                .WithMessage("A metadata entry with the same key as the file 'Name' must exist.")
                .Must(file =>
                    !string.IsNullOrWhiteSpace(file.ContentType) &&
                    MediaMaps.MimeTypeToCategory.ContainsKey(file.ContentType))
                .WithMessage("File 'ContentType' is not allowed or not recognized.")
                .When(c => c.Files != null && c.Files.All(f => f is not null));
        }
    }

    public sealed class Handler : ICommandHandler<Command>
    {
        private readonly MashkoorContext _context;
        private readonly TimeProvider _timeProvider;
        private readonly IStorageManager _storage;
        private readonly IProducer _producer;
        private readonly IIdentityInfo _identity;
        private readonly IStrLoc _l;

        public Handler(
            MashkoorContext context,
            TimeProvider timeProvider,
            IStorageManager storage,
            IProducer producer,
            IIdentityInfo identity,
            IStrLoc l)
        {
            _context = context;
            _timeProvider = timeProvider;
            _storage = storage;
            _producer = producer;
            _identity = identity;
            _l = l;
        }

        public async Task<IResult> Handle([NotNull] Command cmd, CancellationToken ctk)
        {
            var batchId = Guid.NewGuid();
            var medias = new MediaFile[cmd.Files.Length];

            for (var i = 0; i < cmd.Files.Length; i++)
            {
                var file = cmd.Files[i];
                var metadata = cmd.Metadata[file.Name];

                var targetEntity = MediaMaps.MediaTypeToEntity[metadata.Type];

                if (targetEntity is nameof(Customer))
                {
                    medias[i] = MediaFile.CreateCustomerMedia(
                        _storage,
                        batchId,
                        _timeProvider.UtcNow(),
                        metadata.Type,
                        file.ContentType,
                        _identity.Id);
                }
                // TODO: For other types, ensure the current auth user owns the entity & the entity exists:
                // await _context.TheEntity.ContainsAsync(p => p.Id == cmd.TargetId && p.UserId == _identity.Id);
                else
                {
                    return Result.BadRequest(_l[$"Invalid target entity '{targetEntity}' for media type '{metadata.Type}'"]);
                }
            }

            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                await _context.MediaFiles.AddRangeAsync(medias, ctk);
                await _context.SaveChangesAsync(ctk);

                var uploadEvent = await MediaUploaded.CreateAsync(_identity, medias, cmd.Files);
                await _producer.PublishAsync(uploadEvent, ctk);

                await transaction.CommitAsync();
                return Result.Accepted(value: new BatchIdObj(batchId));
            });
        }
    }
}
