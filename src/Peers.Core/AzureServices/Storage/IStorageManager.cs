namespace Peers.Core.AzureServices.Storage;

/// <summary>
/// Azure storage contract.
/// </summary>
public interface IStorageManager
{
    /// <summary>
    /// Creates a public container with the specific name if it does not exist.
    /// </summary>
    /// <param name="containerName">The container name.</param>
    /// <returns></returns>
    Task CreateContainerAsync(string containerName);
    /// <summary>
    /// Deletes the specified container if exists.
    /// </summary>
    /// <param name="containerName">The container name.</param>
    /// <returns></returns>
    Task DeleteContainerAsync(string containerName);
    /// <summary>
    /// Uploads the specified stream into the specified container with the specified blob path and sets the specified content type.
    /// </summary>
    /// <param name="containerName">The container name.</param>
    /// <param name="blobName">The blob name.</param>
    /// <param name="contentType">The content type of the blob.</param>
    /// <param name="stream">The blob's stream.</param>
    /// <returns>The blob's uri if the operation is successful; null otherwise.</returns>
    Task<Uri?> UploadAsync(
        string containerName,
        string blobName,
        string contentType,
        Stream stream);
    /// <summary>
    /// Deletes the blob with the specified URI if exists.
    /// </summary>
    /// <param name="blobUri">The blob's URI.</param>
    /// <returns></returns>
    Task DeleteAsync(Uri blobUri);
    /// <summary>
    /// Returns the URI of the specified blob.
    /// </summary>
    /// <param name="containerName">The container name.</param>
    /// <param name="blobName">The blob name.</param>
    /// <returns></returns>
    Uri GetBlobUri(string containerName, string blobName);
    /// <summary>
    /// Extracts the container and blob name from the given URL.
    /// </summary>
    /// <param name="uri">The url.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    (string containerName, string blobName) ExtractContainerAndBlob(Uri uri);
}
