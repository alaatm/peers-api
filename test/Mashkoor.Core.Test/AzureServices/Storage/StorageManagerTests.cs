using System.Text;
using Azure.Storage.Blobs;
using Mashkoor.Core.AzureServices.Configuration;
using Mashkoor.Core.AzureServices.Storage;
using Microsoft.Extensions.Logging;

namespace Mashkoor.Core.Test.AzureServices.Storage;

public class StorageManagerTests : IAsyncLifetime
{
    private readonly StorageManager _manager;
    private readonly string _container = "test-container";
    private readonly string _blobName = "folder1/test.txt";

    public StorageManagerTests()
    {
        var config = new AzureConfig
        {
            StorageConnectionString = "UseDevelopmentStorage=true",
            DevTunnelsUri = null
        };

        _manager = new StorageManager(config, Mock.Of<ILogger<StorageManager>>());
    }

    public async Task InitializeAsync()
    {
        var containerClient = new BlobContainerClient("UseDevelopmentStorage=true", _container);
        await containerClient.DeleteIfExistsAsync();
    }

    public async Task DisposeAsync()
    {
        var containerClient = new BlobContainerClient("UseDevelopmentStorage=true", _container);
        await containerClient.DeleteIfExistsAsync();
    }

    [Fact]
    public async Task Can_create_and_delete_containers()
    {
        // Arrange
        var containerName = Guid.NewGuid().ToString("N");
        var serviceClient = new BlobServiceClient("UseDevelopmentStorage=true");

        // Act & assert
        await _manager.CreateContainerAsync(containerName);
        Assert.Contains(serviceClient.GetBlobContainers(), p => p.Name == containerName);
        await _manager.DeleteContainerAsync(containerName);
        Assert.DoesNotContain(serviceClient.GetBlobContainers(), p => p.Name == containerName);
    }

    [Fact]
    public async Task Can_upload_and_download_blobs()
    {
        // Arrange
        await _manager.CreateContainerAsync(_container);

        var content = "Hello, Azurite!";
        using var uploadStream = new MemoryStream(Encoding.UTF8.GetBytes(content));

        // Act & assert
        var blobUri = await _manager.UploadAsync(_container, _blobName, "text/plain", uploadStream);
        Assert.NotNull(blobUri);

        using var downloadStream = new MemoryStream();
        await _manager.DownloadAsync(blobUri, downloadStream);

        var result = Encoding.UTF8.GetString(downloadStream.ToArray());
        Assert.Equal(content, result);
    }

    [Fact]
    public async Task UploadAsync_returns_blobUri_on_success()
    {
        // Arrange
        await _manager.CreateContainerAsync(_container);

        var content = "Hello, Azurite!";
        using var uploadStream = new MemoryStream(Encoding.UTF8.GetBytes(content));

        // Act
        var blobUri = await _manager.UploadAsync(_container, _blobName, "text/plain", uploadStream);

        // Assert
        Assert.EndsWith($"{_container}/{_blobName}", blobUri.ToString());
    }

    [Fact]
    public async Task UploadAsync_returns_null_on_failure()
    {
        // Arrange
        var config = new AzureConfig
        {
            StorageConnectionString = @"DefaultEndpointsProtocol=https;AccountName=dummy;AccountKey=ZHVtbXk=;EndpointSuffix=core.windows.net",
        };
        var manager = new StorageManager(config, Mock.Of<ILogger<StorageManager>>());

        var content = "Hello, Azurite!";
        using var uploadStream = new MemoryStream(Encoding.UTF8.GetBytes(content));

        // Act
        var blobUri = await manager.UploadAsync(_container, _blobName, "text/plain", uploadStream);

        // Assert
        Assert.Null(blobUri);
    }

    [Fact]
    public async Task DeleteAsync_deletes_blob()
    {
        // Arrange
        await _manager.CreateContainerAsync(_container);

        using var uploadStream = new MemoryStream(Encoding.UTF8.GetBytes("To be deleted"));
        var blobUri = await _manager.UploadAsync(_container, _blobName, "text/plain", uploadStream);
        Assert.NotNull(blobUri);

        // Act
        await _manager.DeleteAsync(blobUri);

        // Assert
        using var outputStream = new MemoryStream();
        await _manager.DownloadAsync(blobUri, outputStream);
        Assert.Equal(0, outputStream.Length); // Nothing downloaded
    }

    [Fact]
    public void GetBlobUri_returns_expected_Uri()
    {
        // Arrange, act & assert
        var blobUri = _manager.GetBlobUri(_container, _blobName);
        Assert.EndsWith($"{_container}/{_blobName}", blobUri.ToString());
    }

    [Fact]
    public void GetBlobUri_returns_expected_Uri_when_using_devTunnels()
    {
        // Arrange
        var config = new AzureConfig
        {
            StorageConnectionString = "UseDevelopmentStorage=true",
            DevTunnelsUri = new Uri("https://abc.net"),
        };
        var manager = new StorageManager(config, Mock.Of<ILogger<StorageManager>>());

        // Act
        var blobUri = manager.GetBlobUri(_container, _blobName);

        // Assert
        Assert.Equal($"https://abc.net/{_container}/{_blobName}", blobUri.ToString());
    }

    [Theory]
    [InlineData("https://localhost")]
    [InlineData("https://localhost/")]
    [InlineData("https://localhost/test-container")]
    [InlineData("https://localhost/test-container/")]
    public void ExtractContainerAndBlob_throws_on_invalid_Uri(string url)
    {
        // Arrange
        var uri = new Uri(url);

        // Act & assert
        var ex = Assert.Throws<ArgumentException>(() => StorageManager.ExtractContainerAndBlob(uri));
        Assert.Equal("The URI does not contain both a container name and a blob name.", ex.Message);
    }

    [Fact]
    public void ExtractContainerAndBlob_returns_container_and_blobName()
    {
        // Arrange
        var blobUri = _manager.GetBlobUri("mycontainer", "some/path/blob.txt");

        // Act
        var (container, blob) = StorageManager.ExtractContainerAndBlob(blobUri);

        // Assert
        Assert.Equal("mycontainer", container);
        Assert.Equal("some/path/blob.txt", blob);
    }

    [Fact]
    public async Task TryAzOperationAsync_handles_AggregateException()
    {
        // Arrange
        var uri = new Uri("https://localhost/container/blob");

        // Act & assert
        var result = await _manager.TryAzOperationAsync("TestAggregate", uri, () =>
            Task.FromException(new AggregateException(new Azure.RequestFailedException(404, "Not found"))));

        Assert.False(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task UploadAsync_throws_on_invalid_inputs(string invalidName)
    {
        using var stream = new MemoryStream(new byte[10]);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _manager.UploadAsync(invalidName, "blob", "text/plain", stream));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _manager.UploadAsync("container", invalidName, "text/plain", stream));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _manager.UploadAsync("container", "blob", invalidName, stream));
    }

    [Fact]
    public async Task UploadAsync_throws_on_null_inputs()
    {
        using var stream = new MemoryStream(new byte[10]);

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _manager.UploadAsync(null, "blob", "text/plain", stream));

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _manager.UploadAsync("container", null, "text/plain", stream));

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _manager.UploadAsync("container", "blob", null, stream));

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _manager.UploadAsync("container", "blob", "text/plain", null));
    }
}
