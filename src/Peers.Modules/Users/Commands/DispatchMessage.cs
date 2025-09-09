using Peers.Core.Communication;
using Peers.Core.Communication.Push;
using Peers.Core.Cqrs.Pipeline;
using Peers.Core.Localization;

namespace Peers.Modules.Users.Commands;

public static class DispatchMessage
{
    /// <summary>
    /// The command.
    /// </summary>
    /// <param name="DeviceToken">The firebase token of the device to send the message to.</param>
    /// <param name="Title">The message title in all supported languages.</param>
    /// <param name="Body">The message body in all supported languages.</param>
    /// <returns></returns>
    [Authorize(Roles = Roles.UsersManager)]
    public sealed record Command(
        string? DeviceToken,
        Dictionary<string, string> Title,
        Dictionary<string, string> Body) : ICommand, IValidatable;

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(p => p.Title).NotNull().NotEmpty();
            RuleFor(p => p.Body).NotNull().NotEmpty();
        }
    }

    public sealed class Handler : ICommandHandler<Command>
    {
        private readonly PeersContext _context;
        private readonly IPushNotificationService _push;
        private readonly IStrLoc _l;

        public Handler(
            PeersContext context,
            IPushNotificationService push,
            IStrLoc l)
        {
            _context = context;
            _push = push;
            _l = l;
        }

        public async Task<IResult> Handle([NotNull] Command cmd, CancellationToken ctk)
        {
            foreach (var lang in Lang.SupportedLanguages)
            {
                if (!cmd.Title.ContainsKey(lang))
                {
                    return Result.BadRequest(_l["Title must be set for all supported languages."]);
                }
                if (!cmd.Body.ContainsKey(lang))
                {
                    return Result.BadRequest(_l["Body must be set for all supported languages."]);
                }
            }

            var msgBuilder = MessageBuilder.Create(_l);

            if (cmd.DeviceToken is not null)
            {
                var user = await _context
                    .Users
                    .AsNoTracking()
                    .Where(p => p.DeviceList.Any(q => q.PnsHandle == cmd.DeviceToken))
                    .SingleOrDefaultAsync(ctk);

                if (user is null)
                {
                    return Result.BadRequest(_l["User not found."]);
                }

                msgBuilder.Add(cmd.DeviceToken, cmd.Title[user.PreferredLanguage].Trim(), cmd.Body[user.PreferredLanguage].Trim());
            }
            else
            {
                foreach (var lang in Lang.SupportedLanguages)
                {
                    var title = cmd.Title[lang].Trim();
                    var body = cmd.Body[lang].Trim();

                    msgBuilder.Topic($"{FirebaseMessagingServiceExtensions.CustomersTopic}-{lang}", title, body);
                }
            }

            await _push.DispatchAsync(msgBuilder.Generate());
            return Result.NoContent();
        }
    }
}
