using System.Diagnostics;
using Peers.Modules.Media.Domain;
using Microsoft.IO;
using FormFile = Peers.Core.Http.FormFile;

namespace Peers.Modules.Media.Processing;

public sealed class MediaUploaded : TraceableNotification
{
    private const int BlockSize = 1024;
    private const int LargeBufferMultiple = 1024 * 1024;
    private const int MaxBufferSize = 16 * LargeBufferMultiple;

    private static readonly RecyclableMemoryStreamManager _manager = new(options: new(
        blockSize: BlockSize,
        largeBufferMultiple: LargeBufferMultiple,
        maximumBufferSize: MaxBufferSize,
        maximumLargePoolFreeBytes: MaxBufferSize * 4,
        maximumSmallPoolFreeBytes: 100 * BlockSize)
    {
#if DEBUG
        GenerateCallStacks = true,
#endif
        AggressiveBufferReturn = true,
    });

    /// <summary>
    /// The upload files.
    /// </summary>
    /// <value></value>
    public MediaFile[] Files { get; set; }

    private MediaUploaded(
        [NotNull] IIdentityInfo identityInfo,
        MediaFile[] files) : base(identityInfo)
        => Files = files;

    /// <summary>
    /// Creates a new instance of <see cref="MediaUploaded"/>.
    /// </summary>
    /// <param name="identityInfo">The identity info.</param>
    /// <param name="mediaFiles">The media files.</param>
    /// <param name="files">The upload files.</param>
    public static async Task<MediaUploaded> CreateAsync(
        [NotNull] IIdentityInfo identityInfo,
        [NotNull] MediaFile[] mediaFiles,
        [NotNull] FormFile[] files)
    {
        await CopyFilesAsync(files, mediaFiles);
        return new MediaUploaded(identityInfo, mediaFiles);
    }

    private static async Task CopyFilesAsync(FormFile[] sourceFiles, MediaFile[] targetFiles)
    {
        Debug.Assert(sourceFiles.Length == targetFiles.Length);

        for (var i = 0; i < sourceFiles.Length; i++)
        {
            var source = sourceFiles[i];
            var target = targetFiles[i];

            Debug.Assert(target.Stream is null);
            Debug.Assert(target.Thumbnail?.Stream is null);

            var mem = _manager.GetStream(null, source.Stream.Length);
            await source.Stream.CopyToAsync(mem);
            mem.Position = 0;

            target.Stream = mem;
            target.Thumbnail?.Stream = _manager.GetStream();
        }
    }
}
