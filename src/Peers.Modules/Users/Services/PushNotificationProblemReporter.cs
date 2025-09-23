using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Peers.Core.Communication.Push;

namespace Peers.Modules.Users.Services;

/// <summary>
/// Writes all errors to the database.
/// </summary>
public class PushNotificationProblemReporter : IPushNotificationProblemReporter
{
    private readonly PeersContext _context;
    private readonly TimeProvider _timeProvider;

    public PushNotificationProblemReporter(
        PeersContext context,
        TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    /// <summary>
    /// Reports all specified errors.
    /// </summary>
    /// <param name="errors">The errors.</param>
    /// <returns></returns>
    public async Task ReportErrorsAsync(
        List<(string, ErrorCode, MessagingErrorCode?)> errors)
    {
        await _context.PushNotificationProblems.AddRangeAsync(errors
            .Select(p => new Domain.PushNotificationProblem
            {
                Token = p.Item1,
                ReportedOn = _timeProvider.UtcNow(),
                ErrorCode = p.Item2,
                MessagingErrorCode = p.Item3,
            }));

        await _context.SaveChangesAsync();
    }
}
