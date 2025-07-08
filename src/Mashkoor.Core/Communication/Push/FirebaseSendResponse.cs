using FirebaseAdmin.Messaging;

namespace Mashkoor.Core.Communication.Push;

/// <summary>
/// Represents a firebase send response.
/// </summary>
public sealed class FirebaseSendResponse
{
    /// <summary>
    /// The message id.
    /// </summary>
    /// <value></value>
    public string? MessageId { get; }
    /// <summary>
    /// The exception.
    /// </summary>
    /// <value></value>
    public FirebaseMessagingException? Exception { get; }
    /// <summary>
    /// Indicates whether the request was successful.
    /// </summary>
    /// <returns></returns>
    public bool IsSuccess => !string.IsNullOrEmpty(MessageId);

    /// <summary>
    /// Creates a success response.
    /// </summary>
    /// <param name="messageId">The message id.</param>
    internal FirebaseSendResponse(string messageId) => MessageId = messageId;
    /// <summary>
    /// Creates a failure response.
    /// </summary>
    /// <param name="exception">The exception.</param>
    internal FirebaseSendResponse(FirebaseMessagingException exception) => Exception = exception;
}
