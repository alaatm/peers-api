using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace Mashkoor.Core.Media;

/// <summary>
/// Represents a service for generating thumbnails for media files.
/// </summary>
public sealed class ThumbnailGenerator : IThumbnailGenerator
{
    private static readonly JpegEncoder _jpegEncoder = new()
    {
        Quality = 75,
    };

    /// <summary>
    /// Generates a thumbnail from the input stream and writes it to the output stream. Resets the position of both streams to 0 after processing.
    /// </summary>
    /// <param name="input">The input stream containing the media file.</param>
    /// <param name="output">The output stream where the thumbnail will be written.</param>
    /// <param name="width">The desired width of the thumbnail. The height will be adjusted to maintain the aspect ratio.</param>
    /// <returns>The size of the generated thumbnail in bytes.</returns>
    public async Task<int> GenerateThumbnailAsync(
        Stream input,
        Stream output,
        int width = 320)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentOutOfRangeException.ThrowIfLessThan(width, 1);

        try
        {
            input.Position = 0;
            using var image = await Image.LoadAsync(input);

            if (image.Width > width)
            {
                // Preserve aspect ratio; ImageSharp will compute height
                image.Mutate(x => x.Resize(new Size(width, 0)));
            }

            await image.SaveAsJpegAsync(output, _jpegEncoder);
            return (int)output.Position;
        }
        catch (Exception ex) when (ex is
            NotSupportedException or
            InvalidImageContentException or
            UnknownImageFormatException or
            ImageProcessingException)
        {
            input.Position = 0;
            await input.CopyToAsync(output);
            return (int)input.Length;
        }
        finally
        {
            input.Position = output.Position = 0;
        }
    }
}
