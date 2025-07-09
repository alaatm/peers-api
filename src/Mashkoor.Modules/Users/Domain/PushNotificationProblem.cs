using FirebaseAdmin;
using FirebaseAdmin.Messaging;

namespace Mashkoor.Modules.Users.Domain;

public class PushNotificationProblem : Entity, IAggregateRoot
{
    /// <summary>
    /// The token with the problem.
    /// </summary>
    public string Token { get; set; } = default!;
    /// <summary>
    /// Date/time when the problem was reported.
    /// </summary>
    public DateTime ReportedOn { get; set; }
    /// <summary>
    /// The error code.
    /// </summary>
    public ErrorCode ErrorCode { get; set; }
    /// <summary>
    /// The messaging error code.
    /// </summary>
    public MessagingErrorCode? MessagingErrorCode { get; set; }
}
