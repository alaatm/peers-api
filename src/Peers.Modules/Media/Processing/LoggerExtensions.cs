namespace Peers.Modules.Media.Processing;

[ExcludeFromCodeCoverage]
internal static partial class LoggerExtensions
{
    [LoggerMessage(LogLevel.Information, "Processing {FilesCount} media uploads.", SkipEnabledCheck = true)]
    public static partial void ProcessingUploads(this ILogger logger, int filesCount);

    [LoggerMessage(LogLevel.Warning, "Failed to upload all {FilesCount} media files.", SkipEnabledCheck = true)]
    public static partial void AllUploadsFailed(this ILogger logger, int filesCount);

    [LoggerMessage(LogLevel.Warning, "Failed to upload {ErrorsCount} out of {FilesCount} media uploads.", SkipEnabledCheck = true)]
    public static partial void PartialUploadsFailed(this ILogger logger, int filesCount, int errorsCount);

    [LoggerMessage(LogLevel.Information, "All {FilesCount} media uploads succeeded.", SkipEnabledCheck = true)]
    public static partial void AllUploadsSucceeded(this ILogger logger, int filesCount);
}
