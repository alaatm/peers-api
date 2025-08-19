using Mashkoor.Core.AzureServices.Storage;
using Mashkoor.Core.Http;
using Mashkoor.Core.Identity;
using Mashkoor.Modules.Media.Domain;
using Mashkoor.Modules.Media.Processing;

namespace Mashkoor.Modules.Test.Media.Processing;

public class MediaUploadedTests : DomainEntityTestBase
{
    [Fact]
    public async Task CreateAsync_copies_files_streams()
    {
        // Arrange
        var formFile = new FormFile() { Name = "name1", ContentType = "image/jpeg", Stream = new MemoryStream([5]) };
        var mediaFile = MediaFile.CreateCustomerMedia(Mock.Of<IStorageManager>(), Guid.NewGuid(), DateTime.UtcNow, MediaType.ProfilePicture, "image/jpeg", 1);

        // Act
        var e = await MediaUploaded.CreateAsync(Mock.Of<IIdentityInfo>(), [mediaFile], [formFile]);

        // Assert
        Assert.NotNull(mediaFile.Stream);
        Assert.Equal(0, mediaFile.Stream.Position);
        Assert.Equal(1, mediaFile.Stream.Length);
        Assert.Equal(5, mediaFile.Stream.ReadByte());

        Assert.Contains(e.Files, f => f == mediaFile);
    }

    [Fact]
    public async Task CreateAsync_initializes_thumbnail_stream_when_a_thumbnail_exist()
    {
        // Arrange
        var formFile = new FormFile() { Name = "name1", ContentType = "image/jpeg", Stream = new MemoryStream([5]) };
        var mediaFile = MediaFile.CreateCustomerMedia(Mock.Of<IStorageManager>(), Guid.NewGuid(), DateTime.UtcNow, MediaType.ProfilePicture, "image/jpeg", 1);
        mediaFile.Thumbnail = new MediaFile();

        // Act
        await MediaUploaded.CreateAsync(Mock.Of<IIdentityInfo>(), [mediaFile], [formFile]);

        // Assert
        Assert.NotNull(mediaFile.Thumbnail.Stream);
    }
}
