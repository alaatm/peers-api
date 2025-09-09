using System.Diagnostics.CodeAnalysis;
using FirebaseAdmin.Messaging;

namespace Peers.Core.Communication.Push;

public sealed class FirebaseMessagingWrapper : IFirebaseMessagingWrapper
{
    /// <summary>
    /// The inner Firebase Messaging.
    /// </summary>
    public FirebaseMessaging? Client { get; set; }

    /// <summary>
    /// Sends a message to the FCM service for delivery. The message gets validated both
    /// by the Admin SDK, and the remote FCM service. A successful return value indicates
    /// that the message has been successfully sent to FCM, where it has been accepted
    /// by the FCM service.
    /// </summary>
    /// <param name="message">The message to be sent. Must not be null.</param>
    /// <returns></returns>
    [ExcludeFromCodeCoverage]
    public Task<string> SendAsync(Message message)
        => Client!.SendAsync(message);

    /// <summary>
    /// Sends each message in the given list via Firebase Cloud Messaging. Unlike FirebaseAdmin.Messaging.FirebaseMessaging.SendAllAsync(System.Collections.Generic.IEnumerable{FirebaseAdmin.Messaging.Message}),
    /// this method makes an HTTP call for each message in the given list.
    /// </summary>
    /// <param name="messages">Up to 500 messages to send in the batch. Cannot be null or empty.</param>
    /// <returns></returns>
    [ExcludeFromCodeCoverage]
    public Task<BatchResponse> SendEachAsync(IEnumerable<Message> messages)
        => Client!.SendEachAsync(messages);

    /// <summary>
    /// Sends the given multicast message to all the FCM registration tokens specified
    /// in it. Unlike FirebaseAdmin.Messaging.FirebaseMessaging.SendMulticastAsync(FirebaseAdmin.Messaging.MulticastMessage),
    /// this method makes an HTTP call for each token in the given multicast message.
    /// </summary>
    /// <param name="message">The message to be sent. Must not be null or empty.</param>
    /// <returns></returns>
    [ExcludeFromCodeCoverage]
    public Task<BatchResponse> SendEachForMulticastAsync(MulticastMessage message)
        => Client!.SendEachForMulticastAsync(message);

    /// <summary>
    /// Subscribes a list of registration tokens to a topic.
    /// </summary>
    /// <param name="registrationTokens">A list of registration token to subscribe.</param>
    /// <param name="topic">The topic name to subscribe to. /topics/ will be prepended to the topic name provided if absent.</param>
    /// <returns></returns>
    [ExcludeFromCodeCoverage]
    public Task<TopicManagementResponse> SubscribeToTopicAsync(IReadOnlyList<string> registrationTokens, string topic)
        => Client!.SubscribeToTopicAsync(registrationTokens, topic);

    /// <summary>
    /// Unsubscribes a list of registration tokens from a topic.
    /// </summary>
    /// <param name="registrationTokens">A list of registration tokens to unsubscribe.</param>
    /// <param name="topic">The topic name to unsubscribe from. /topics/ will be prepended to the topic name provided if absent.</param>
    /// <returns></returns>
    [ExcludeFromCodeCoverage]
    public Task<TopicManagementResponse> UnsubscribeFromTopicAsync(IReadOnlyList<string> registrationTokens, string topic)
        => Client!.UnsubscribeFromTopicAsync(registrationTokens, topic);
}
