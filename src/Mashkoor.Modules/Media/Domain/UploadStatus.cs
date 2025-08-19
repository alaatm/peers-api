namespace Mashkoor.Modules.Media.Domain;

/// <summary>
/// Represents the status of an upload operation.
/// </summary>
public enum UploadStatus
{
    /// <summary>
    /// The upload is pending.
    /// </summary>
    Pending = 0,
    /// <summary>
    /// The upload is in progress.
    /// </summary>
    InProgress = 1,
    /// <summary>
    /// The upload has been completed successfully.
    /// </summary>
    Completed = 2,
    /// <summary>
    /// The upload has failed.
    /// </summary>
    Failed = 3
}
