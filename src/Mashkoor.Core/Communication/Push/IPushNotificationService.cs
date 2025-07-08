using FirebaseAdmin.Messaging;

namespace Mashkoor.Core.Communication.Push;

/// <summary>
/// Push notification service contract.
/// </summary>
public interface IPushNotificationService
{
    /// <summary>
    /// Dispatches all specified messages using underlying firebase service.
    /// </summary>
    /// <param name="messageGroup">The message group to dispatch.</param>
    /// <returns></returns>
    Task<(IReadOnlyList<Message>, IReadOnlyList<MulticastMessage>)> DispatchAsync(
        (IReadOnlyList<Message>, IReadOnlyList<MulticastMessage>) messageGroup);
}
