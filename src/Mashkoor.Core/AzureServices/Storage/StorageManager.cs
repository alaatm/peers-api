using System.Diagnostics.CodeAnalysis;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Mashkoor.Core.AzureServices.Configuration;

namespace Mashkoor.Core.AzureServices.Storage;

public sealed class StorageManager : IStorageManager
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly ILogger<StorageManager> _log;
    private readonly Uri _publicBaseUri;

    public StorageManager([NotNull] AzureConfig config, ILogger<StorageManager> log)
    {
        _log = log;

        _blobServiceClient = new BlobServiceClient(config.StorageConnectionString, new BlobClientOptions
        {
            Retry =
            {
                NetworkTimeout = TimeSpan.FromSeconds(5),
                MaxRetries = 3,
                MaxDelay = TimeSpan.FromSeconds(2),
                Delay = TimeSpan.FromSeconds(1),
                Mode = Azure.Core.RetryMode.Fixed
            }
        });

        var realBaseUri = _blobServiceClient.Uri;

        _publicBaseUri = config.DevTunnelsUri is null
            ? realBaseUri
            : new UriBuilder(realBaseUri)
            {
                Scheme = config.DevTunnelsUri.Scheme,
                Host = config.DevTunnelsUri.Host,
                Port = -1
            }.Uri;
    }

    /// <summary>
    /// Creates a public container with the specific name if it does not exist.
    /// </summary>
    /// <param name="containerName">The container name.</param>
    /// <returns></returns>
    public Task CreateContainerAsync(string containerName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(containerName, nameof(containerName));

        return _blobServiceClient
            .GetBlobContainerClient(containerName)
            .CreateIfNotExistsAsync(PublicAccessType.Blob);
    }

    /// <summary>
    /// Deletes the specified container if exists.
    /// </summary>
    /// <param name="containerName">The container name.</param>
    /// <returns></returns>
    public Task DeleteContainerAsync(string containerName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(containerName, nameof(containerName));

        return _blobServiceClient
            .GetBlobContainerClient(containerName)
            .DeleteIfExistsAsync();
    }

    /// <summary>
    /// Uploads the specified stream into the specified container with the specified blob path and sets the specified content type.
    /// </summary>
    /// <param name="containerName">The container name.</param>
    /// <param name="blobName">The blob name.</param>
    /// <param name="contentType">The content type of the blob.</param>
    /// <param name="stream">The blob's stream.</param>
    /// <returns>The blob's uri if the operation is successful; null otherwise.</returns>
    public async Task<Uri?> UploadAsync(
        string containerName,
        string blobName,
        string contentType,
        Stream stream)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(containerName, nameof(containerName));
        ArgumentException.ThrowIfNullOrWhiteSpace(blobName, nameof(blobName));
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType, nameof(contentType));
        ArgumentNullException.ThrowIfNull(stream, nameof(stream));

        var blobUri = GetBlobUri(containerName, blobName);
        var blobClient = GetBlobClient(blobUri);
        var uploadOptions = new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
        };

        _log.UploadingBlob(containerName, blobName, Math.Round(stream.Length / 1024d, 2));

        if (await TryAzOperationAsync("Upload", blobUri, () => blobClient.UploadAsync(stream, uploadOptions)))
        {
            return blobUri;
        }

        return null;
    }

    /// <summary>
    /// Deletes the blob with the specified URI if exists.
    /// </summary>
    /// <param name="blobUri">The blob's URI.</param>
    /// <returns></returns>
    public async Task DeleteAsync(Uri blobUri)
    {
        var blobClient = GetBlobClient(blobUri);

        _log.DeletingBlob(blobUri);

        await TryAzOperationAsync("Delete", blobUri, () => blobClient.DeleteIfExistsAsync());
    }

    /// <summary>
    /// Returns the URI of the specified blob.
    /// </summary>
    /// <param name="containerName">The container name.</param>
    /// <param name="blobName">The blob name.</param>
    /// <returns></returns>
    public Uri GetBlobUri(string containerName, string blobName)
        => new UriBuilder(_publicBaseUri)
        {
            Path = $"{containerName}/{blobName}"
        }.Uri;

    internal async Task<bool> TryAzOperationAsync(string operationName, Uri blobUri, Func<Task> operation)
    {
        try
        {
            await operation();
            return true;
        }
        catch (RequestFailedException ex)
        {
            _log.BlobOperationFailed(operationName, blobUri, ex.Status, ex.ErrorCode, ex.Message);
            return false;
        }
        catch (AggregateException ae) when (ae.InnerExceptions.All(e => e is RequestFailedException))
        {
            foreach (var ex in ae.InnerExceptions.Cast<RequestFailedException>())
            {
                _log.BlobOperationFailed(operationName, blobUri, ex.Status, ex.ErrorCode, ex.Message); // Log each failure
            }
            return false;
        }
    }

    internal static (string containerName, string blobName) ExtractContainerAndBlob(Uri uri)
    {
        // Get the path, e.g. "/container-name/a/b/c.x"
        var path = uri.AbsolutePath.AsSpan();

        // Trim leading slash
        if (path.StartsWith('/'))
        {
            path = path[1..];
        }

        // Find the first slash (end of container name)
        var slashIndex = path.IndexOf('/');

        if (slashIndex == -1 || slashIndex == path.Length - 1)
        {
            throw new ArgumentException("The URI does not contain both a container name and a blob name.");
        }

        var containerNameSpan = path[..slashIndex];
        var blobNameSpan = path[(slashIndex + 1)..];

        return (containerNameSpan.ToString(), blobNameSpan.ToString());
    }

    private BlobClient GetBlobClient(Uri blobUri)
    {
        ArgumentNullException.ThrowIfNull(blobUri, nameof(blobUri));

        var (containerName, blobName) = ExtractContainerAndBlob(blobUri);
        return _blobServiceClient
            .GetBlobContainerClient(containerName)
            .GetBlobClient(blobName);
    }
}
