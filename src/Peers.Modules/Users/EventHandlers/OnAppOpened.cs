using Peers.Modules.Users.Events;

namespace Peers.Modules.Users.EventHandlers;

public sealed class OnAppOpened : INotificationHandler<AppOpened>
{
    private readonly PeersContext _context;

    public OnAppOpened(PeersContext context) => _context = context;

    public async Task Handle([NotNull] AppOpened notification, CancellationToken ctk)
    {
        var user = await _context
            .Users
            .FirstAsync(p => p.Id == notification.UserId, ctk);

        user.RecordAppUsed(notification.Date);
        await _context.SaveChangesAsync(ctk);
    }
}
