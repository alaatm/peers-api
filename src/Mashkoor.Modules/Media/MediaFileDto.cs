using Mashkoor.Modules.Media.Domain;

namespace Mashkoor.Modules.Media;

/// <summary>
/// Represents a media file with associated metadata.
/// </summary>
/// <remarks>This data transfer object encapsulates information about a media file, including its URL, type,
/// category, and upload status.</remarks>
/// <param name="Url">The URL of the media file.</param>
/// <param name="Description">Optional description of the media file.</param>
/// <param name="Type">The type of the media file, such as profile picture, etc.</param>
/// <param name="Category">The category of the media file, such as image, video, etc.</param>
/// <param name="Status">The status of the media upload, indicating whether it is pending, completed, or failed.</param>
/// <param name="ThumbnailUrl">Optional thumbnail URL for the media file, if available.</param>
public sealed record MediaFileDto(
    Uri Url,
    string? Description,
    MediaType Type,
    MediaCategory Category,
    UploadStatus Status,
    Uri? ThumbnailUrl);

public static class MediaFileQueryableExtensions
{
    extension(IQueryable<MediaFile> q)
    {
        public IQueryable<MediaFileDto> ProjectToDto()
            => q.Select(p => new MediaFileDto(
                p.MediaUrl,
                p.Description,
                p.Type,
                p.Category,
                p.Status,
                p.Thumbnail != null && p.Thumbnail.Status == UploadStatus.Completed
                    ? p.Thumbnail.MediaUrl
                    : null));
    }
}
