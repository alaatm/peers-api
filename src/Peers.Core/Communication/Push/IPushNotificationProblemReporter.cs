using FirebaseAdmin.Messaging;
using FirebaseAdmin;

namespace Peers.Core.Communication.Push;

/// <summary>
/// Abstraction over reporting push notification problems.
/// </summary>
public interface IPushNotificationProblemReporter
{
    /// <summary>
    /// Reports all specified errors.
    /// </summary>
    /// <param name="errors">The errors.</param>
    /// <returns></returns>
    Task ReportErrorsAsync(List<(string, ErrorCode, MessagingErrorCode?)> errors);
}
