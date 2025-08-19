using Mashkoor.Core.AzureServices.Storage;
using Mashkoor.Modules.Media.Domain;
using Mashkoor.Modules.Media.Rules;

namespace Mashkoor.Modules.Test.Media.Domain;

public class MediaFileTests : DomainEntityTestBase
{
    [Fact]
    public void CreateCustomerMedia_creates_media_for_customer()
    {
        // Arrange
        var storageMoq = new Mock<IStorageManager>();
        var batchId = Guid.NewGuid();
        var date = DateTime.UtcNow;
        var type = MediaType.ProfilePicture;
        var contentType = "image/jpeg";
        var customerId = 1;

        var expectedBlobUrl = "https://fakestorage.blob.core.windows.net/blobName";

        storageMoq
            .Setup(p => p.GetBlobUri(MediaFile.ContainerName, It.IsAny<string>()))
            .Returns(new Uri(expectedBlobUrl))
            .Verifiable();

        // Act
        var media = MediaFile.CreateCustomerMedia(storageMoq.Object, batchId, date, type, contentType, customerId);

        // Assert
        Assert.NotNull(media);
        Assert.Equal(batchId, media.BatchId);
        Assert.Equal(date, media.CreatedAt);
        Assert.Equal(type, media.Type);
        Assert.Null(media.Description);
        Assert.Equal(contentType, media.ContentType);
        Assert.Equal(customerId, media.CustomerId);
        Assert.Equal(UploadStatus.Pending, media.Status);
        Assert.True(media.Approved);
        Assert.Equal(expectedBlobUrl, media.MediaUrl.ToString());
        Assert.Equal(MediaCategory.Image, media.Category);

        // Thumbnail
        var thumbnail = media.Thumbnail;
        Assert.NotNull(thumbnail);
        Assert.Same(thumbnail.Original, media);
        Assert.Equal(batchId, thumbnail.BatchId);
        Assert.Equal(date, thumbnail.CreatedAt);
        Assert.Equal(type, thumbnail.Type);
        Assert.Null(thumbnail.Description);
        Assert.Equal(contentType, thumbnail.ContentType);
        Assert.Equal(customerId, thumbnail.CustomerId);
        Assert.Equal(UploadStatus.Pending, thumbnail.Status);
        Assert.True(thumbnail.Approved);
        Assert.Equal(expectedBlobUrl, thumbnail.MediaUrl.ToString());
        Assert.Equal(MediaCategory.Image, media.Category);

        storageMoq.VerifyAll();
    }

    [Fact]
    public void CreateCustomerMedia_checks_business_rules()
    {
        // Arrange & act
        var media = GetTestMediaFile();

        // Assert
        Assert.IsType<ContentTypeCategoryMatchRule>(Assert.Single(media.CheckedRules));
    }

    [Fact]
    public void MarkInProgress_sets_status_to_in_progress()
    {
        // Arrange
        var media = GetTestMediaFile();
        Assert.Equal(UploadStatus.Pending, media.Status);

        // Act
        media.MarkInProgress();

        // Assert
        Assert.Equal(UploadStatus.InProgress, media.Status);
    }

    [Fact]
    public void MarkCompleted_sets_status_to_completed()
    {
        // Arrange
        var media = GetTestMediaFile();
        media.MarkInProgress();

        // Act
        media.MarkCompleted();

        // Assert
        Assert.Equal(UploadStatus.Completed, media.Status);
    }

    [Fact]
    public void MarkFailed_sets_status_to_failed()
    {
        // Arrange
        var media = GetTestMediaFile();
        media.MarkInProgress();

        // Act
        media.MarkFailed();

        // Assert
        Assert.Equal(UploadStatus.Failed, media.Status);
    }

    [Fact]
    public void Reject_sets_approved_to_false_and_thumbnail_approved_to_false()
    {
        // Arrange
        var media = GetTestMediaFile();
        media.MarkInProgress();
        media.MarkCompleted();

        // Act
        media.Reject();

        // Assert
        Assert.False(media.Approved);
        Assert.False(media.Thumbnail.Approved);
    }

    [Fact]
    public void Reject_sets_approved_to_false()
    {
        // Arrange
        var media = new MediaFile();
        media.MarkInProgress();
        media.MarkCompleted();

        // Act
        media.Reject();

        // Assert
        Assert.False(media.Approved);
        Assert.Null(media.Thumbnail?.Approved);
    }

    private static MediaFile GetTestMediaFile()
        => MediaFile.CreateCustomerMedia(Mock.Of<IStorageManager>(), Guid.NewGuid(), DateTime.UtcNow, MediaType.ProfilePicture, "image/jpeg", 1);
}
