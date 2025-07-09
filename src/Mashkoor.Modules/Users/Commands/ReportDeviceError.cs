using Mashkoor.Core.Cqrs.Pipeline;
using Mashkoor.Modules.Users.Domain;

namespace Mashkoor.Modules.Users.Commands;

public static class ReportDeviceError
{
    /// <summary>
    /// The command.
    /// </summary>
    /// <param name="DeviceId"></param>
    /// <param name="Username"></param>
    /// <param name="Locale"></param>
    /// <param name="Silent"></param>
    /// <param name="Source"></param>
    /// <param name="AppVersion"></param>
    /// <param name="AppState"></param>
    /// <param name="Exception"></param>
    /// <param name="StackTrace"></param>
    /// <param name="Info"></param>
    /// <param name="DeviceInfo"></param>
    /// <param name="Key"></param>
    public sealed record Command(
        Guid DeviceId,
        string? Username,
        string? Locale,
        bool Silent,
        string? Source,
        string? AppVersion,
        string? AppState,
        string? Exception,
        string? StackTrace,
        string? Info,
        string? DeviceInfo,
        string Key) : ICommand, IValidatable;

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(p => p.DeviceId).NotEmpty();
            RuleFor(p => p.Key).NotEmpty();
        }
    }

    public sealed class Handler : ICommandHandler<Command>
    {
        public const string Key = "d00920b2-ae48-402b-997e-32d9f42345fe";

        private readonly MashkoorContext _context;
        private readonly TimeProvider _timeProvider;
        private readonly ILogger<Handler> _log;

        public Handler(
            MashkoorContext context,
            TimeProvider timeProvider,
            ILogger<Handler> log)
        {
            _context = context;
            _timeProvider = timeProvider;
            _log = log;
        }

        public async Task<IResult> Handle([NotNull] Command cmd, CancellationToken ctk)
        {
            if (cmd.Key != Key)
            {
                return Result.Unauthorized("Missing or invalid key.");
            }

            _log.DeviceCrashReported(cmd.Silent);

            var entity = DeviceError.Create
            (
                _timeProvider.UtcNow(),
                cmd.DeviceId,
                cmd.Username,
                cmd.Locale,
                cmd.Silent,
                cmd.Source,
                cmd.AppVersion,
                cmd.AppState,
                cmd.Exception,
                cmd.StackTrace is null ? [] : cmd.StackTrace.Split('\n', StringSplitOptions.RemoveEmptyEntries),
                cmd.Info is null ? [] : cmd.Info.Split('\n', StringSplitOptions.RemoveEmptyEntries),
                cmd.DeviceInfo
            );

            await _context
                .DevicesErrors
                .AddAsync(entity, ctk);

            await _context.SaveChangesAsync(ctk);
            return Result.Created(value: new IdObj(entity.Id));
        }
    }
}
