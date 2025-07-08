using System.Diagnostics.CodeAnalysis;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Mashkoor.Core.Communication.Push.Configuration;

namespace Mashkoor.Core.Communication.Push;

/// <summary>
/// A firebase service forwarding proxy.
/// </summary>
public sealed class FirebaseMessagingService : IFirebaseMessagingService
{
    public const string FirebaseAppName = "MashkoorFirebaseApp";

    private static readonly Lock _lock = new();
    private readonly IFirebaseMessagingWrapper _firebase;

    public FirebaseMessagingService(
        [NotNull] FirebaseConfig config,
        IFirebaseMessagingWrapper firebase)
    {
        _firebase = firebase;
        SetFirebaseMessagingClient(config);
    }

    /// <summary>
    /// Sends a message to the FCM service for delivery. The message gets validated both
    /// by the Admin SDK, and the remote FCM service. A successful return value indicates
    /// that the message has been successfully sent to FCM, where it has been accepted
    /// by the FCM service.
    /// </summary>
    /// <param name="message">The message to be sent. Must not be null.</param>
    /// <returns></returns>
    public async Task<FirebaseSendResponse> SendAsync([NotNull] Message message)
    {
        try
        {
            var messageId = await _firebase.SendAsync(message);
            return new FirebaseSendResponse(messageId);
        }
        catch (FirebaseMessagingException ex)
        {
            return new FirebaseSendResponse(ex);
        }
    }

    /// <summary>
    /// Sends all the messages in the given list via Firebase Cloud Messaging. Employs
    /// batching to send the entire list as a single RPC call. Compared to the <see cref="SendAsync(Message)"/>
    /// method, this is a significantly more efficient way to send multiple messages.
    /// </summary>
    /// <param name="messages">Up to 500 messages to send in the batch. Cannot be null.</param>
    /// <returns></returns>
    public Task<BatchResponse> SendAllAsync([NotNull] IEnumerable<Message> messages) => _firebase.SendEachAsync(messages);

    /// <summary>
    /// Sends the given multicast message to all the FCM registration tokens specified
    /// in it.
    /// </summary>
    /// <param name="message">The message to be sent. Must not be null.</param>
    /// <returns></returns>
    public Task<BatchResponse> SendMulticastAsync(MulticastMessage message) => _firebase.SendEachForMulticastAsync(message);

    /// <summary>
    /// Subscribes the given registration token to a topic.
    /// </summary>
    /// <param name="registrationToken">The registration token.</param>
    /// <param name="topic">The topic.</param>
    /// <returns></returns>
    public Task<TopicManagementResponse> SubscribeToTopicAsync(string registrationToken, string topic) => _firebase.SubscribeToTopicAsync([registrationToken], topic);

    /// <summary>
    /// Unsubscribes the given registration token from a topic.
    /// </summary>
    /// <param name="registrationToken">The registration token.</param>
    /// <param name="topic">The topic.</param>
    /// <returns></returns>
    public Task<TopicManagementResponse> UnsubscribeFromTopicAsync(string registrationToken, string topic) => _firebase.UnsubscribeFromTopicAsync([registrationToken], topic);

    private void SetFirebaseMessagingClient(FirebaseConfig config)
    {
        if (string.IsNullOrEmpty(config.ServiceAccountKey))
        {
            throw new InvalidOperationException("Firebase service account key is not set.");
        }

        var app = FirebaseApp.GetInstance(FirebaseAppName);
        if (app is null)
        {
            lock (_lock)
            {
                app = FirebaseApp.GetInstance(FirebaseAppName) ?? FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromJson(config.ServiceAccountKey),
                }, FirebaseAppName);
            }
        }

        _firebase.Client = FirebaseMessaging.GetMessaging(app);
    }
}
