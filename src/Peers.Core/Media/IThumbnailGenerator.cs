namespace Peers.Core.Media;

/// <summary>
/// Represents a service for generating thumbnails for media files.
/// </summary>
public interface IThumbnailGenerator
{
    /// <summary>
    /// Generates a thumbnail from the input stream and writes it to the output stream.
    /// </summary>
    /// <param name="input">The input stream containing the media file.</param>
    /// <param name="output">The output stream where the thumbnail will be written.</param>
    /// <param name="width">The desired width of the thumbnail. The height will be adjusted to maintain the aspect ratio.</param>
    /// <returns>The size of the generated thumbnail in bytes.</returns>
    Task<int> GenerateThumbnailAsync(
        Stream input,
        Stream output,
        int width = 320);
}
