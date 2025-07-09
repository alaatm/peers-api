using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Mashkoor.Core.Communication.Push;

namespace Mashkoor.Modules.Users.Services;

/// <summary>
/// Writes all errors to the database.
/// </summary>
public class PushNotificationProblemReporter : IPushNotificationProblemReporter
{
    private readonly MashkoorContext _context;
    private readonly TimeProvider _timeProvider;

    public PushNotificationProblemReporter(
        MashkoorContext context,
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
        ICollection<(string, ErrorCode, MessagingErrorCode?)> errors)
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
