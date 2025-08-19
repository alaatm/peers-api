using System.Diagnostics;
using Mashkoor.Core.AzureServices.Storage;
using Mashkoor.Core.Media;

namespace Mashkoor.Modules.Media.Processing;

public sealed class HandleMediaUploaded : INotificationHandler<MediaUploaded>
{
    private readonly MashkoorContext _context;
    private readonly IStorageManager _storage;
    private readonly IThumbnailGenerator _thumbnailGenerator;
    private readonly ILogger<HandleMediaUploaded> _log;

    public HandleMediaUploaded(
        MashkoorContext context,
        IStorageManager storage,
        IThumbnailGenerator thumbnailGenerator,
        ILogger<HandleMediaUploaded> log)
    {
        _context = context;
        _storage = storage;
        _thumbnailGenerator = thumbnailGenerator;
        _log = log;
    }

    public async Task Handle([NotNull] MediaUploaded notification, CancellationToken ctk)
    {
        if (notification.Files.Length == 0)
        {
            return;
        }

        var mediaFilesWithStreams =
            notification.Files
            .SelectMany(p => p.Thumbnail is not null ? [p, p.Thumbnail] : new[] { p })
            .ToArray();

        var mediaFiles = (await _context
            .MediaFiles
            .Include(p => p.Thumbnail)
            .Where(p => notification.Files.Select(f => f.Id).Contains(p.Id))
            .ToArrayAsync(ctk))
            .SelectMany(p => p.Thumbnail is not null ? [p, p.Thumbnail] : new[] { p })
            .ToArray();

        foreach (var file in mediaFiles)
        {
            file.MarkInProgress();
        }

        await _context.SaveChangesAsync(ctk);

        var uploadTasks = new Task<Uri?>[mediaFiles.Length];
        _log.ProcessingUploads(mediaFiles.Length);

        for (var i = 0; i < mediaFiles.Length; i++)
        {
            var mediaFile = mediaFiles[i];
            var (container, blobName) = _storage.ExtractContainerAndBlob(mediaFile.MediaUrl);

            // Retrieve the stream for the media file
            mediaFile.Stream = mediaFilesWithStreams.First(p => p.MediaUrl == mediaFile.MediaUrl).Stream!;

            mediaFile.SizeInBytes = mediaFile.Original is null
                ? mediaFile.Stream.Length
                : await _thumbnailGenerator.GenerateThumbnailAsync(mediaFile.Original.Stream!, mediaFile.Stream);

            uploadTasks[i] = _storage.UploadAsync(
                container,
                blobName,
                mediaFile.ContentType,
                mediaFile.Stream);
        }

        var urls = await Task.WhenAll(uploadTasks);
        var errorCount = urls.Count(p => p is null);
        var succeeded = urls.Where(p => p is not null).ToArray();

        foreach (var file in mediaFiles)
        {
            Debug.Assert(file.Stream is not null);
            await file.Stream.DisposeAsync();

            if (succeeded.FirstOrDefault(p => p!.AbsolutePath == file.MediaUrl.AbsolutePath) is not null)
            {
                file.MarkCompleted();
            }
            else
            {
                file.MarkFailed();
            }
        }

        await _context.SaveChangesAsync(ctk);

        if (errorCount == mediaFiles.Length)
        {
            _log.AllUploadsFailed(mediaFiles.Length);
        }
        else if (errorCount > 0)
        {
            _log.PartialUploadsFailed(mediaFiles.Length, errorCount);
        }
        else if (errorCount == 0)
        {
            _log.AllUploadsSucceeded(mediaFiles.Length);
        }
    }
}
