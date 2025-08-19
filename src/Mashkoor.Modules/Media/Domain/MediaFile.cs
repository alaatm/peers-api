using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using Mashkoor.Core.AzureServices.Storage;
using Mashkoor.Modules.Customers.Domain;
using Mashkoor.Modules.Media.Rules;

namespace Mashkoor.Modules.Media.Domain;

/// <summary>
/// Represents a media file.
/// </summary>
public sealed class MediaFile : Entity, IAggregateRoot
{
    public const string ContainerName = "media";

    /// <summary>
    /// The batch identifier for the media upload, used to group related uploads.
    /// </summary>
    public Guid BatchId { get; set; }
    /// <summary>
    /// The URL of the media file.
    /// </summary>
    public Uri MediaUrl { get; private set; } = default!;
    /// <summary>
    /// The date and time when the media file was created.
    /// </summary>
    public DateTime CreatedAt { get; private set; }
    /// <summary>
    /// An optional description of the media file.
    /// </summary>
    public string? Description { get; set; }
    /// <summary>
    /// Indicates whether the media file has been approved.
    /// </summary>
    public bool Approved { get; set; }
    /// <summary>
    /// The type of the media file, such as profile picture, etc..
    /// </summary>
    public MediaType Type { get; private set; }
    /// <summary>
    /// The category of the media file, such as image, video, etc..
    /// </summary>
    public MediaCategory Category { get; private set; }
    /// <summary>
    /// The content type of the media file, such as image/jpeg, video/mp4, etc..
    /// </summary>
    public string ContentType { get; private set; } = default!;
    /// <summary>
    /// The size of the media file in bytes.
    /// </summary>
    public long? SizeInBytes { get; set; }
    /// <summary>
    /// The status of the media upload.
    /// </summary>
    public UploadStatus Status { get; private set; }
    /// <summary>
    /// The identifier of the thumbnail media file, if available.
    /// </summary>
    public int? ThumbnailId { get; set; }
    /// <summary>
    /// The thumbnail of the media file, if available.
    /// </summary>
    public MediaFile? Thumbnail { get; set; }
    /// <summary>
    /// The original media file, if this is a thumbnail or a derivative.
    /// </summary>
    public MediaFile? Original { get; set; }

    /// <summary>
    /// The customer id who uploaded the media file.
    /// </summary>
    public int CustomerId { get; private set; }
    /// <summary>
    /// The customer who uploaded the media file.
    /// </summary>
    public Customer Customer { get; private set; } = default!;

    /// <summary>
    /// The file stream attached when performing actual upload.
    /// </summary>
    [NotMapped]
    public Stream? Stream { get; set; }

    internal MediaFile() { }

    private MediaFile(
        IStorageManager storage,
        Guid batchId,
        DateTime date,
        string? description,
        MediaType type,
        string contentType,
        int customerId)
    {
        BatchId = batchId;
        MediaUrl = storage.GetBlobUri(ContainerName, Guid.NewGuid().ToString("N"));
        CreatedAt = date;
        Description = description;
        Approved = true;
        Type = type;
        ContentType = contentType;
        Status = UploadStatus.Pending;
        CustomerId = customerId;
    }

    public static MediaFile CreateCustomerMedia(
        [NotNull] IStorageManager storage,
        Guid batchId,
        DateTime date,
        MediaType type,
        string contentType,
        int customerId)
    {
        Debug.Assert(MediaMaps.MediaTypeToEntity[type] == nameof(Customer));
        return Create(storage, batchId, date, null, type, contentType, customerId);
    }

    private static MediaFile Create(
        IStorageManager storage,
        Guid batchId,
        DateTime date,
        string? description,
        MediaType type,
        string contentType,
        int customerId)
    {
        var media = new MediaFile(storage, batchId, date, description, type, contentType, customerId);

        if (MediaMaps.MediaTypeToCategory.TryGetValue(type, out var category))
        {
            media.Category = category;

            if (category is MediaCategory.Image)
            {
                media.CreateThumbnail(storage);
            }
        }

        media.CheckRule(new ContentTypeCategoryMatchRule(contentType, media.Category));

        return media;
    }

    /// <summary>
    /// Marks the media upload as in progress.
    /// </summary>
    public void MarkInProgress()
    {
        Debug.Assert(Status == UploadStatus.Pending, "Upload must be pending to mark as in progress.");
        Status = UploadStatus.InProgress;
    }

    /// <summary>
    /// Marks the media upload as completed successfully.
    /// </summary>
    public void MarkCompleted()
    {
        Debug.Assert(Status == UploadStatus.InProgress, "Upload must be in progress to mark as completed.");
        Status = UploadStatus.Completed;
    }

    /// <summary>
    /// Marks the media upload as failed.
    /// </summary>
    public void MarkFailed()
    {
        Debug.Assert(Status == UploadStatus.InProgress, "Upload must be in progress to mark as failed.");
        Status = UploadStatus.Failed;
    }

    public void Reject()
    {
        Debug.Assert(Status == UploadStatus.Completed, "Upload must be completed to reject.");
        Approved = false;
        Thumbnail?.Approved = false;
    }

    private MediaFile CreateThumbnail(IStorageManager storage)
    {
        Debug.Assert(Category is MediaCategory.Image, "Only image media can have thumbnails.");
        Debug.Assert(Thumbnail is null, "Thumbnail must be null to create a new one.");

        Thumbnail = new MediaFile(storage, BatchId, CreatedAt, null, Type, "image/jpeg", CustomerId)
        {
            Category = MediaCategory.Image,
            Original = this
        };

        return Thumbnail;
    }
}
