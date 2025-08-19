using Mashkoor.Core.Http;
using Mashkoor.Modules.Media.Commands;
using Mashkoor.Modules.Media.Domain;
using Mashkoor.Modules.Media.Processing;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Mashkoor.Modules.Test.Media.Commands.Handlers;

[Collection(nameof(IntegrationTestBaseCollection))]
public class UploadTests : IntegrationTestBase
{
    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Requires_Customer_role()
        => await AssertCommandAccess(TestUploadCommand(-1), [Roles.Customer]);

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Creates_mediaEntity_and_publishes_mediaUploadedEvent()
    {
        // Arrange
        var customer = await EnrollCustomer();
        ProducerMoq
            .Setup(p => p.PublishAsync(It.IsAny<MediaUploaded>(), It.IsAny<CancellationToken>()))
            .Verifiable();

        // Act
        var result = await SendAsync(TestUploadCommand(), customer);

        // Assert
        var acceptedResult = AssertX.IsType<Accepted<Upload.BatchIdObj>>(result);
        var batchId = acceptedResult.Value.BatchId;

        ExecuteDbContext(db =>
        {
            var uploadedMedia = db.MediaFiles.Where(p => p.BatchId == batchId).ToArray();
            Assert.Equal(2, uploadedMedia.Length); // image and thumbnail
            Assert.All(uploadedMedia, mf => Assert.Equal(customer.Id, mf.CustomerId));
            Assert.All(uploadedMedia, mf => Assert.Equal(MediaType.ProfilePicture, mf.Type));
            Assert.All(uploadedMedia, mf => Assert.Equal(UploadStatus.Pending, mf.Status));
            Assert.All(uploadedMedia, mf => Assert.Equal("image/jpeg", mf.ContentType));
        });

        ProducerMoq.VerifyAll();
    }

    public static Upload.Command TestUploadCommand(int? targetId = null, MediaType mediaType = MediaType.ProfilePicture, string description = null) => new(
        targetId,
        new Dictionary<string, Upload.Command.FileMetadata>
        {
            { "file1", new Upload.Command.FileMetadata(mediaType, description) },
        },
        [new FormFile() { Stream = new MemoryStream(), Name = "file1", ContentType = "image/jpeg" }]);
}
