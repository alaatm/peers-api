using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Microsoft.EntityFrameworkCore;
using Peers.Modules.Users.Services;

namespace Peers.Modules.Test.Users.Services;

[Collection(nameof(IntegrationTestBaseCollection))]
public class PushNotificationProblemReporterTests : IntegrationTestBase
{
    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task ReportErrors_inserts_all_entries() =>
        await ExecuteDbContextAsync(async db =>
        {
            // Arrange
            var service = new PushNotificationProblemReporter(db, TimeProvider.System);

            // Act
            await service.ReportErrorsAsync(new List<(string, ErrorCode, MessagingErrorCode?)>
            {
                { ("111", ErrorCode.NotFound, null) },
                { ("222", ErrorCode.AlreadyExists, MessagingErrorCode.Unregistered) },
            });

            // Assert
            var entries = await db.PushNotificationProblems.ToArrayAsync();
            Assert.Equal(2, entries.Length);

            Assert.Equal("111", entries[0].Token);
            Assert.Equal(ErrorCode.NotFound, entries[0].ErrorCode);
            Assert.Null(entries[0].MessagingErrorCode);

            Assert.Equal("222", entries[1].Token);
            Assert.Equal(ErrorCode.AlreadyExists, entries[1].ErrorCode);
            Assert.Equal(MessagingErrorCode.Unregistered, entries[1].MessagingErrorCode);
        });
}
