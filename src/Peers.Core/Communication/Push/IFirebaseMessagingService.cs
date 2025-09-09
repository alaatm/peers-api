using FirebaseAdmin.Messaging;

namespace Peers.Core.Communication.Push;

/// <summary>
/// A firebase service contract with 1 to 1 method signature match.
/// </summary>
public interface IFirebaseMessagingService
{
    /// <summary>
    /// Sends a message to the FCM service for delivery. The message gets validated both
    /// by the Admin SDK, and the remote FCM service. A successful return value indicates
    /// that the message has been successfully sent to FCM, where it has been accepted
    /// by the FCM service.
    /// </summary>
    /// <param name="message">The message to be sent. Must not be null.</param>
    /// <returns></returns>
    Task<FirebaseSendResponse> SendAsync(Message message);
    /// <summary>
    /// Sends all the messages in the given list via Firebase Cloud Messaging. Employs
    /// batching to send the entire list as a single RPC call. Compared to the <see cref="SendAsync(Message)"/>
    /// method, this is a significantly more efficient way to send multiple messages.
    /// </summary>
    /// <param name="messages">Up to 500 messages to send in the batch. Cannot be null.</param>
    /// <returns></returns>
    Task<BatchResponse> SendAllAsync(IEnumerable<Message> messages);
    /// <summary>
    /// Sends the given multicast message to all the FCM registration tokens specified
    /// in it.
    /// </summary>
    /// <param name="message">The message to be sent. Must not be null.</param>
    /// <returns></returns>
    Task<BatchResponse> SendMulticastAsync(MulticastMessage message);
    /// <summary>
    /// Subscribes the given registration token to a topic.
    /// </summary>
    /// <param name="registrationToken">The registration token.</param>
    /// <param name="topic">The topic.</param>
    /// <returns></returns>
    Task<TopicManagementResponse> SubscribeToTopicAsync(string registrationToken, string topic);
    /// <summary>
    /// Unsubscribes the given registration token from a topic.
    /// </summary>
    /// <param name="registrationToken">The registration token.</param>
    /// <param name="topic">The topic.</param>
    /// <returns></returns>
    Task<TopicManagementResponse> UnsubscribeFromTopicAsync(string registrationToken, string topic);
}
