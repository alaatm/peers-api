using Peers.Modules.Media.Commands;
using Peers.Modules.Media.Processing;
using Peers.Modules.Media.Queries;
using Peers.Modules.Test.Media.Commands.Handlers;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Peers.Modules.Test.Media.Queries;

[Collection(nameof(IntegrationTestBaseCollection))]
public class GetStatusTests : IntegrationTestBase
{
    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Requires_Customer_role()
        => await AssertCommandAccess(new GetStatus.Query(default), [Roles.Customer]);

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Returns_NotFound_for_invalid_batchId()
    {
        // Arrange
        var customer = await EnrollCustomer();

        // Act
        var result = await SendAsync(new GetStatus.Query(Guid.NewGuid()), customer);

        // Assert
        AssertX.IsType<NotFound>(result);
    }

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Returns_status_for_valid_batchId()
    {
        // Arrange
        ProducerMoq
            .Setup(p => p.PublishAsync(It.IsAny<MediaUploaded>(), It.IsAny<CancellationToken>()))
            .Verifiable();
        var customer = await EnrollCustomer();
        var uploadResult = AssertX.IsType<Accepted<Upload.BatchIdObj>>(await SendAsync(UploadTests.TestUploadCommand(), customer));

        // Act
        var result = await SendAsync(new GetStatus.Query(uploadResult.Value.BatchId), customer);

        // Assert
        var okResult = AssertX.IsType<Ok<GetStatus.Response>>(result);
        Assert.Equal(2, okResult.Value.Status.Count);
    }
}
