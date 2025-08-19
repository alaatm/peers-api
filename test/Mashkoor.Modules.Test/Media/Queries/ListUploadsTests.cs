using Mashkoor.Core.Queries;
using Mashkoor.Modules.Media.Commands;
using Mashkoor.Modules.Media.Domain;
using Mashkoor.Modules.Media.Processing;
using Mashkoor.Modules.Media.Queries;
using Mashkoor.Modules.Test.Media.Commands.Handlers;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Mashkoor.Modules.Test.Media.Queries;

[Collection(nameof(IntegrationTestBaseCollection))]
public class ListUploadsTests : IntegrationTestBase
{
    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Requires_Staff_role()
        => await AssertCommandAccess(new ListUploads.Query(default, default, default, default, default), [Roles.Staff]);

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Returns_upload_list()
    {
        // Arrange
        var manager = await InsertManagerAsync();

        ProducerMoq
            .Setup(p => p.PublishAsync(It.IsAny<MediaUploaded>(), It.IsAny<CancellationToken>()))
            .Verifiable();
        var customer = await EnrollCustomer();
        AssertX.IsType<Accepted<Upload.BatchIdObj>>(await SendAsync(UploadTests.TestUploadCommand(), customer));

        // Act
        var result = await SendAsync(new ListUploads.Query(default, default, default, default, default), manager);

        // Assert
        var okResult = AssertX.IsType<Ok<PagedQueryResponse<ListUploads.Response>>>(result);
        Assert.Equal(1, okResult.Value.Total);
        var upload = Assert.Single(okResult.Value.Data);

        Assert.Equal(customer.Id, upload.UploadedBy.Key);
        Assert.Equal(customer.Username, upload.UploadedBy.Value);
        Assert.Null(upload.Description);
        Assert.Equal(MediaType.ProfilePicture, upload.Type);
        Assert.Equal(MediaCategory.Image, upload.Category);
        Assert.Equal(UploadStatus.Pending, upload.Status);
    }
}
