using Mashkoor.Core.Media;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;

namespace Mashkoor.Core.Test.Media;

public class ThumbnailGeneratorTests
{
    [Fact]
    public async Task GenerateThumbnailAsync_throws_on_null_input()
    {
        // Arrange
        var generator = new ThumbnailGenerator();
        using var output = new MemoryStream();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await generator.GenerateThumbnailAsync(null!, output, 100));
    }

    [Fact]
    public async Task GenerateThumbnailAsync_throws_on_null_output()
    {
        // Arrange
        var generator = new ThumbnailGenerator();
        using var input = GetTestImageStream();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await generator.GenerateThumbnailAsync(input, null!, 100));
    }

    [Fact]
    public async Task GenerateThumbnailAsync_throws_on_invalid_size()
    {
        // Arrange
        var generator = new ThumbnailGenerator();
        using var input = GetTestImageStream();
        using var output = new MemoryStream();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            await generator.GenerateThumbnailAsync(input, output, 0));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            await generator.GenerateThumbnailAsync(input, output, -10));
    }

    [Fact]
    public async Task GenerateThumbnailAsync_should_generate_thumbnail()
    {
        // Arrange
        var generator = new ThumbnailGenerator();
        using var input = GetTestImageStream();
        using var output = new MemoryStream();

        // Act
        var size = await generator.GenerateThumbnailAsync(input, output, 100);

        // Assert
        Assert.Equal(output.Length, size);
        Assert.Equal(0, input.Position);
        Assert.Equal(0, output.Position);

        using var image = Image.Load(output);
        Assert.Equal(100, image.Width);
        Assert.Equal(100, image.Height);
    }

    [Fact]
    public async Task GenerateThumbnailAsync_ensures_input_stream_position_is_at_start()
    {
        // Arrange
        var generator = new ThumbnailGenerator();
        using var input = GetTestImageStream();
        using var output = new MemoryStream();

        // Act
        input.Seek(input.Length, SeekOrigin.Begin);
        var size = await generator.GenerateThumbnailAsync(input, output, 100);

        // Assert
        Assert.Equal(output.Length, size);
        Assert.Equal(0, input.Position);
        Assert.Equal(0, output.Position);

        using var image = Image.Load(output);
        Assert.Equal(100, image.Width);
        Assert.Equal(100, image.Height);
    }

    [Fact]
    public async Task GenerateThumbnailAsync_should_not_resize_small_image()
    {
        // Arrange
        var generator = new ThumbnailGenerator();
        using var input = GetTestImageStream(width: 100, height: 100);
        using var output = new MemoryStream();

        // Act
        var size = await generator.GenerateThumbnailAsync(input, output, 120);

        // Assert
        Assert.Equal(output.Length, size);
        Assert.Equal(0, input.Position);
        Assert.Equal(0, output.Position);

        using var image = Image.Load(output);
        Assert.Equal(100, image.Width);
        Assert.Equal(100, image.Height);
    }

    [Fact]
    public async Task GenerateThumbnailAsync_should_return_original_on_invalid_image()
    {
        // Arrange
        var generator = new ThumbnailGenerator();
        using var input = new MemoryStream([1, 2, 3, 4, 5]);
        using var output = new MemoryStream();

        // Act
        var size = await generator.GenerateThumbnailAsync(input, output, 100);

        // Assert
        Assert.Equal(output.Length, size);
        Assert.Equal(0, input.Position);
        Assert.Equal(0, output.Position);
        Assert.Equal(5, output.Length);
        Assert.Equal(new byte[] { 1, 2, 3, 4, 5 }, output.ToArray());
    }

    private static MemoryStream GetTestImageStream(int width = 400, int height = 400)
    {
        var inputImage = new MemoryStream();
        using var image = Image.WrapMemory<Rgba32>(new byte[width * height * 8], width, height);
        image.Save(inputImage, new JpegEncoder());
        inputImage.Position = 0;
        return inputImage;
    }
}
