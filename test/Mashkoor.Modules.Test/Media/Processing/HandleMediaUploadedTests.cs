using Mashkoor.Core.AzureServices.Storage;
using Mashkoor.Core.Http;
using Mashkoor.Core.Identity;
using Mashkoor.Core.Media;
using Mashkoor.Modules.Media.Domain;
using Mashkoor.Modules.Media.Processing;
using Microsoft.Extensions.Logging;

namespace Mashkoor.Modules.Test.Media.Processing;

internal enum UploadFailMode
{
    None,
    Partial,
    Full
}

public class HandleMediaUploadedTests : IntegrationTestBase
{
    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Noops_when_no_files_in_notification()
    {
        // Arrange
        var handler = new HandleMediaUploaded(null, null, null, null);
        var notification = await MediaUploaded.CreateAsync(Mock.Of<IIdentityInfo>(), [], []);

        // Act
        await handler.Handle(notification, default);

        // Assert
        // No exceptions thrown
    }

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Uploads_files_successfully()
    {
        // Arrange
        var storageMoq = GetStorageMock(UploadFailMode.None);
        var thumbnailGenMoq = GetThumbnailGenMock();

        var batchId = Guid.NewGuid();
        var customer = await EnrollCustomer();

        var formFile = new FormFile() { Name = "name1", ContentType = "image/jpeg", Stream = new MemoryStream([5, 6]) };
        var mediaFile = MediaFile.CreateCustomerMedia(storageMoq.Object, batchId, DateTime.UtcNow, MediaType.ProfilePicture, "image/jpeg", customer.Id);
        var notification = await MediaUploaded.CreateAsync(Mock.Of<IIdentityInfo>(), [mediaFile], [formFile]);

        ExecuteDbContext(db =>
        {
            db.MediaFiles.Add(mediaFile);
            db.SaveChanges();
        });

        // Act
        await ExecuteDbContextAsync(async db =>
        {
            var handler = new HandleMediaUploaded(db, storageMoq.Object, thumbnailGenMoq.Object, Mock.Of<ILogger<HandleMediaUploaded>>());
            await handler.Handle(notification, default);
        });

        // Assert
        ExecuteDbContext(db =>
        {
            var uploadedMedias = db.MediaFiles.Where(p => p.BatchId == batchId).ToArray();
            foreach (var mf in uploadedMedias)
            {
                Assert.Equal(UploadStatus.Completed, mf.Status);
            }
        });

        storageMoq.VerifyAll();
        thumbnailGenMoq.VerifyAll();
    }

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Handles_partial_failure()
    {
        // Arrange
        var storageMoq = GetStorageMock(UploadFailMode.Partial);
        var thumbnailGenMoq = GetThumbnailGenMock();

        var batchId = Guid.NewGuid();
        var customer = await EnrollCustomer();

        var formFile = new FormFile() { Name = "name1", ContentType = "image/jpeg", Stream = new MemoryStream([5, 6]) };
        var mediaFile = MediaFile.CreateCustomerMedia(storageMoq.Object, batchId, DateTime.UtcNow, MediaType.ProfilePicture, "image/jpeg", customer.Id);
        var notification = await MediaUploaded.CreateAsync(Mock.Of<IIdentityInfo>(), [mediaFile], [formFile]);

        ExecuteDbContext(db =>
        {
            db.MediaFiles.Add(mediaFile);
            db.SaveChanges();
        });

        // Act
        await ExecuteDbContextAsync(async db =>
        {
            var handler = new HandleMediaUploaded(db, storageMoq.Object, thumbnailGenMoq.Object, Mock.Of<ILogger<HandleMediaUploaded>>());
            await handler.Handle(notification, default);
        });

        // Assert
        ExecuteDbContext(db =>
        {
            var uploadedMedias = db.MediaFiles.Where(p => p.BatchId == batchId).ToArray();
            Assert.Contains(uploadedMedias, mf => mf.Status == UploadStatus.Completed);
            Assert.Contains(uploadedMedias, mf => mf.Status == UploadStatus.Failed);
        });
        storageMoq.VerifyAll();
        thumbnailGenMoq.VerifyAll();
    }

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Handles_full_failure()
    {
        // Arrange
        var storageMoq = GetStorageMock(UploadFailMode.Full);
        var thumbnailGenMoq = GetThumbnailGenMock();

        var batchId = Guid.NewGuid();
        var customer = await EnrollCustomer();

        var formFile = new FormFile() { Name = "name1", ContentType = "image/jpeg", Stream = new MemoryStream([5, 6]) };
        var mediaFile = MediaFile.CreateCustomerMedia(storageMoq.Object, batchId, DateTime.UtcNow, MediaType.ProfilePicture, "image/jpeg", customer.Id);
        var notification = await MediaUploaded.CreateAsync(Mock.Of<IIdentityInfo>(), [mediaFile], [formFile]);

        ExecuteDbContext(db =>
        {
            db.MediaFiles.Add(mediaFile);
            db.SaveChanges();
        });

        // Act
        await ExecuteDbContextAsync(async db =>
        {
            var handler = new HandleMediaUploaded(db, storageMoq.Object, thumbnailGenMoq.Object, Mock.Of<ILogger<HandleMediaUploaded>>());
            await handler.Handle(notification, default);
        });

        // Assert
        ExecuteDbContext(db =>
        {
            var uploadedMedias = db.MediaFiles.Where(p => p.BatchId == batchId).ToArray();
            foreach (var mf in uploadedMedias)
            {
                Assert.Equal(UploadStatus.Failed, mf.Status);
            }
        });
        storageMoq.VerifyAll();
        thumbnailGenMoq.VerifyAll();
    }

    private static Mock<IStorageManager> GetStorageMock(UploadFailMode uploadFailMode)
    {
        var callCount = 0;

        var storageMoq = new Mock<IStorageManager>(MockBehavior.Strict);
        storageMoq
            .Setup(p => p.GetBlobUri(MediaFile.ContainerName, It.IsAny<string>()))
            .Returns((string container, string blob) => new Uri($"https://fakestorage.blob.core.windows.net/{container}/{blob}"))
            .Verifiable();
        storageMoq
            .Setup(p => p.ExtractContainerAndBlob(It.IsAny<Uri>()))
            .Returns((Uri uri) => new StorageManager(new() { StorageConnectionString = "UseDevelopmentStorage=true" }, null).ExtractContainerAndBlob(uri))
            .Verifiable();
        storageMoq
            .Setup(p => p.UploadAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>()))
            .ReturnsAsync((string container, string blob, string _, Stream _) => uploadFailMode switch
            {
                UploadFailMode.Full => null,
                UploadFailMode.Partial => callCount++ == 0 ? new Uri($"https://fakestorage.blob.core.windows.net/{container}/{blob}") : null,
                _ => new Uri($"https://fakestorage.blob.core.windows.net/{container}/{blob}")
            })
            .Verifiable();

        return storageMoq;
    }

    private static Mock<IThumbnailGenerator> GetThumbnailGenMock()
    {
        var thumbnailGenMoq = new Mock<IThumbnailGenerator>(MockBehavior.Strict);
        thumbnailGenMoq
            .Setup(p => p.GenerateThumbnailAsync(It.IsAny<Stream>(), It.IsAny<Stream>(), 320))
            .ReturnsAsync(1)
            .Verifiable();

        return thumbnailGenMoq;
    }
}
