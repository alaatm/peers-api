using Peers.Core.AzureServices.Storage;
using Peers.Modules.Media;
using Peers.Modules.Media.Domain;

namespace Peers.Modules.Test.Media;

public class MediaFileQueryableExtensionsTests : DomainEntityTestBase
{
    [Fact]
    public void ProjectToDto_projects_correctly()
    {
        // Arrange
        var file1 = MediaFile.CreateCustomerMedia(Mock.Of<IStorageManager>(), Guid.NewGuid(), DateTime.UtcNow, MediaType.ProfilePicture, "image/jpeg", 1);
        file1.Thumbnail.MarkInProgress();
        file1.Thumbnail.MarkCompleted();

        var file2 = MediaFile.CreateCustomerMedia(Mock.Of<IStorageManager>(), Guid.NewGuid(), DateTime.UtcNow, MediaType.ProfilePicture, "image/jpeg", 1);

        var file3 = MediaFile.CreateCustomerMedia(Mock.Of<IStorageManager>(), Guid.NewGuid(), DateTime.UtcNow, MediaType.ProfilePicture, "image/jpeg", 1);
        file3.Thumbnail = null;

        var files = new List<MediaFile> { file1, file2, file3 }.AsQueryable();

        // Act
        var dtos = files.ProjectToDto().ToList();

        // Assert
        Assert.Equal(3, dtos.Count);
        Assert.Equal(files.ElementAt(0).MediaUrl, dtos[0].Url);
        Assert.Equal(files.ElementAt(0).Description, dtos[0].Description);
        Assert.Equal(files.ElementAt(0).Type, dtos[0].Type);
        Assert.Equal(files.ElementAt(0).Category, dtos[0].Category);
        Assert.Equal(files.ElementAt(0).Status, dtos[0].Status);
        Assert.Equal(files.ElementAt(0).Thumbnail.MediaUrl, dtos[0].ThumbnailUrl);

        Assert.Null(dtos[1].ThumbnailUrl); // Thumbnail status is not completed
        Assert.Null(dtos[2].ThumbnailUrl); // No thumbnail
    }
}
