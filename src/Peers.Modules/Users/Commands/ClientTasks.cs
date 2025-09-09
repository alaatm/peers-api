using System.Diagnostics;
using Humanizer;
using Peers.Core.Communication;
using Peers.Core.Communication.Push;
using Peers.Core.Cqrs.Pipeline;
using Peers.Core.Security.StrongKeys;
using Peers.Modules.Users.Domain;
using Microsoft.Extensions.Caching.Memory;

namespace Peers.Modules.Users.Commands;

public static class ClientTaskRequest
{
    [Authorize(Roles = Roles.UsersManager)]
    public sealed record Command(
        Guid DeviceId,
        RequiredTask? Task,
        string? RequestId) : ICommand, IValidatable;

    public enum RequiredTask
    {
        Ping,
        SignOff,
    }

    public enum AcknowledgeStatus
    {
        Waiting,
        Ok,
        NoResponse,
    }

    public sealed record RequestResponse(string RequestId);

    public sealed record AcknowledgeResponse(AcknowledgeStatus Status);

    public sealed class Validator : AbstractValidator<Command>
    {
        private static readonly string _deviceId = nameof(Command.DeviceId).Humanize();

        public Validator([NotNull] IStrLoc l)
            => RuleFor(p => p.DeviceId).NotEmpty().WithName(l[_deviceId]);
    }

    public sealed class Handler : ICommandHandler<Command>
    {
        private readonly PeersContext _context;
        private readonly IPushNotificationService _push;
        private readonly IMemoryCache _cache;
        private readonly IStrLoc _l;

        public Handler(
            PeersContext context,
            IPushNotificationService push,
            IMemoryCache cache,
            IStrLoc l)
        {
            _context = context;
            _push = push;
            _cache = cache;
            _l = l;
        }

        public async Task<IResult> Handle([NotNull] Command cmd, CancellationToken ctk)
        {
            string key;

            if (string.IsNullOrWhiteSpace(cmd.RequestId))
            {
                Debug.Assert(cmd.Task is not null);

                var device = await _context
                    .Set<Device>()
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(p => p.DeviceId == cmd.DeviceId, ctk);

                if (device is null)
                {
                    return Result.NotFound();
                }
                if (device.PnsHandle is null)
                {
                    return Result.BadRequest(_l["Device is not registered for push notifications."]);
                }

                var requestId = KeyGenerator.Create(12);
                key = $"{device.DeviceId}:{requestId}";
                _cache.Set(key, false, TimeSpan.FromMinutes(15));

                var data = new Dictionary<string, string>
                {
                    { "event", "client-task" },
                    { "task", cmd.Task.Value.ToString().ToUpperInvariant() },
                    { "request-id", requestId },
                };

                await _push.DispatchAsync(MessageBuilder
                    .Create(_l)
                    .Add(device.PnsHandle, data)
                    .Generate());

                return Result.Accepted(value: new RequestResponse(requestId));
            }

            key = $"{cmd.DeviceId}:{cmd.RequestId}";

            if (_cache.TryGetValue(key, out bool value))
            {
                return value
                    ? Result.Ok(new AcknowledgeResponse(AcknowledgeStatus.Ok))
                    : Result.Ok(new AcknowledgeResponse(AcknowledgeStatus.Waiting));
            }
            else
            {
                return Result.Ok(new AcknowledgeResponse(AcknowledgeStatus.NoResponse));
            }
        }
    }
}

public static class ClientTaskAcknowledge
{
    public sealed record Command(
        Guid DeviceId,
        string RequestId) : ICommand;

    public sealed class Handler : ICommandHandler<Command>
    {
        private readonly IMemoryCache _cache;

        public Handler(IMemoryCache cache) => _cache = cache;

        public Task<IResult> Handle([NotNull] Command cmd, CancellationToken ctk)
        {
            var key = $"{cmd.DeviceId}:{cmd.RequestId}";

            if (_cache.TryGetValue(key, out _))
            {
                _cache.Set(key, true, TimeSpan.FromDays(1));
                return Task.FromResult(Result.NoContent());
            }

            return Task.FromResult(Result.NotFound());
        }
    }
}
