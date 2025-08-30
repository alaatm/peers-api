using Mashkoor.Core.Http;
using Mashkoor.Modules.Customers.Commands;
using Mashkoor.Modules.Media.Commands;
using Mashkoor.Modules.Media.Domain;
using Mashkoor.Modules.Media.Processing;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Mashkoor.Modules.Test.Customers.Commands.Handlers;

[Collection(nameof(IntegrationTestBaseCollection))]
public class SetProfilePictureTests : IntegrationTestBase
{
    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Requires_Customer_role()
        => await AssertCommandAccess(new SetProfilePicture.Command(null), [Roles.Customer]);

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Creates_profilePicture()
    {
        // Arrange
        var cmd = new SetProfilePicture.Command(new FormFile() { Stream = new MemoryStream(), Name = "file1", ContentType = "image/jpeg" });
        var customer = await EnrollCustomer();
        ProducerMoq
            .Setup(p => p.PublishAsync(It.IsAny<MediaUploaded>(), It.IsAny<CancellationToken>()))
            .Verifiable();

        // Act
        var result = await SendAsync(cmd, customer);

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

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Replaces_profilePicture_if_one_exist_and_deletes_prev_media()
    {
        // Arrange
        var cmd = new SetProfilePicture.Command(new FormFile() { Stream = new MemoryStream(), Name = "file1", ContentType = "image/jpeg" });
        var customer = await EnrollCustomer();
        ProducerMoq
            .Setup(p => p.PublishAsync(It.IsAny<MediaUploaded>(), It.IsAny<CancellationToken>()))
            .Verifiable();

        var prevBatchId = AssertX.IsType<Accepted<Upload.BatchIdObj>>(await SendAsync(cmd, customer)).Value.BatchId; // first upload        

        // Act
        var result = await SendAsync(cmd, customer);

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

            Assert.Empty(db.MediaFiles.Where(p => p.BatchId == prevBatchId)); // previous media deleted
        });

        ProducerMoq.VerifyAll();
    }
}
