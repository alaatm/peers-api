using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Microsoft.AspNetCore.Http.HttpResults;
using Mashkoor.Core.Queries;
using Mashkoor.Modules.Users.Domain;
using Mashkoor.Modules.Users.Queries;
using Mashkoor.Modules.Users.Services;

namespace Mashkoor.Modules.Test.Users.Queries.Handlers;

[Collection(nameof(IntegrationTestBaseCollection))]
public class ListPushNotificationProblemsTests : IntegrationTestBase
{
    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Requires_usersManager_role()
        => await AssertCommandAccess(TestQuery, [Roles.UsersManager]);

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Returns_failed_dispatches_messages()
    {
        // Arrange
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
        });

        // Act
        var result = await SendAsync(TestQuery, await InsertManagerAsync());

        // Assert
        var okResult = Assert.IsType<Ok<PagedQueryResponse<PushNotificationProblem>>>(result);
        var response = okResult.Value;
        Assert.Equal(2, response.Total);

        Assert.Equal("111", response.Data[0].Token);
        Assert.Equal(ErrorCode.NotFound, response.Data[0].ErrorCode);
        Assert.Null(response.Data[0].MessagingErrorCode);

        Assert.Equal("222", response.Data[1].Token);
        Assert.Equal(ErrorCode.AlreadyExists, response.Data[1].ErrorCode);
        Assert.Equal(MessagingErrorCode.Unregistered, response.Data[1].MessagingErrorCode);
    }

    private static ListPushNotificationProblems.Query TestQuery => new(null, null, null, null, null);
}
