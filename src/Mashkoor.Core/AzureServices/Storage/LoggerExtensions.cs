using System.Diagnostics.CodeAnalysis;

namespace Mashkoor.Core.AzureServices.Storage;

[ExcludeFromCodeCoverage]
internal static partial class LoggerExtensions
{
    [LoggerMessage(LogLevel.Information, "Uploading blob {BlobName} with size {Size} kb to container {ContainerName}.", SkipEnabledCheck = true)]
    public static partial void UploadingBlob(this ILogger logger, string containerName, string blobName, double size);

    [LoggerMessage(LogLevel.Information, "Downloading blob at {Uri}.", SkipEnabledCheck = true)]
    public static partial void DownloadingBlob(this ILogger logger, Uri uri);

    [LoggerMessage(LogLevel.Information, "Deleting blob at {Uri}.", SkipEnabledCheck = true)]
    public static partial void DeletingBlob(this ILogger logger, Uri uri);

    [LoggerMessage(LogLevel.Error, "Blob operation '{OperationName}' failed for blob {BlobUri}. Status: {Status}, ErrorCode: {Code}, Message: {Message}.", SkipEnabledCheck = true)]
    public static partial void BlobOperationFailed(this ILogger logger, string operationName, Uri blobUri, int status, string? code, string message);
}
