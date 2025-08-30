using System.Reflection;
using Mashkoor.Core.AzureServices.Storage;
using Mashkoor.Core.Background;
using Mashkoor.Core.Cqrs.Pipeline;
using Mashkoor.Modules.Media.Domain;

namespace Mashkoor.Modules.Customers.Commands;

public static class SetProfilePicture
{
    ///////// These are for openapi docs only
    [ExcludeFromCodeCoverage]
    public sealed class CommandDoc
    {
        public required IFormFile File { get; init; }
    }
    ///////// These are for openapi docs only

    /// <summary>
    /// Sets customer profile picture.
    /// </summary>
    /// <param name="File">The profile picture file.</param>
    [Authorize(Roles = Roles.Customer)]
    public sealed record Command(
        Core.Http.FormFile? File) : ICommand, IValidatable
    {
        public static async ValueTask<Command?> BindAsync([NotNull] HttpContext context, ParameterInfo _)
        {
            var files = await context.Request.BindFilesAsync();
            return files.Length == 0
                ? new((Core.Http.FormFile?)null)
                : new(files[0]);
        }
    }

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator() => RuleFor(p => p.File).NotNull();
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
            var existingMedia = await _context
                .MediaFiles
                .Include(p => p.Thumbnail)
                .Where(p => p.CustomerId == _identity.Id && p.Type == MediaType.ProfilePicture && p.Original == null)
                .FirstOrDefaultAsync(ctk);

            if (existingMedia is not null)
            {
                var t1 = _context
                    .MediaFiles
                    .Where(p => new int[] { p.Id, p.ThumbnailId!.Value }.Contains(p.Id))
                    .ExecuteDeleteAsync(ctk);

                var t2 = _storage.DeleteAsync(existingMedia.MediaUrl);
                var t3 = _storage.DeleteAsync(existingMedia.Thumbnail!.MediaUrl);

                await Task.WhenAll(t1, t2, t3);
            }

            var uploadCommand = new Media.Commands.Upload.Command(
                null,
                new Dictionary<string, Media.Commands.Upload.Command.FileMetadata>
                {
                    { cmd.File!.Name, new Media.Commands.Upload.Command.FileMetadata(MediaType.ProfilePicture, null) }
                },
                [cmd.File!]);

            return await new Media.Commands.Upload.Handler(
                _context,
                _timeProvider,
                _storage,
                _producer,
                _identity,
                _l)
                .Handle(uploadCommand, ctk);
        }
    }
}
