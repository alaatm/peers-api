using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Mashkoor.Core.Common;

namespace Mashkoor.Core.Http;

public static class HttpRequestExtensions
{
    /// <summary>
    /// Binds form data content, including files, to <see cref="FormData{T}"/>.
    /// </summary>
    /// <typeparam name="T">The model type.</typeparam>
    /// <param name="request">The HTTP request.</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static async Task<FormData<T>> BindFormDataAsync<T>([NotNull] this HttpRequest request) where T : class
    {
        if (!request.ContentType.AsSpan().StartsWith("multipart/form-data"))
        {
            throw new InvalidOperationException($"{nameof(BindFormDataAsync)}<T>() can only be used with form data.");
        }

        var kvp = await request.ReadFormAsync();

        if (kvp.TryGetValue("data", out var tmp) && tmp.ToString() is string kvpData)
        {
            try
            {
                var data = JsonSerializer.Deserialize<T>(kvpData, GlobalJsonOptions.Default);
                var files = BindFiles(kvp);

                return new()
                {
                    Data = data!,
                    Files = files,
                };
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"{nameof(BindFormDataAsync)}<T>() form data 'data' key value must be a valid json string.", ex);
            }
        }

        throw new InvalidOperationException($"{nameof(BindFormDataAsync)}<T>() form data must have a 'data' key with json content.");
    }

    /// <summary>
    /// Binds form data content, including files, to <see cref="FormData{T}"/>.
    /// </summary>
    /// <param name="request">The HTTP request.</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static async Task<FormFile[]> BindFilesAsync([NotNull] this HttpRequest request)
    {
        if (!request.ContentType.AsSpan().StartsWith("multipart/form-data"))
        {
            throw new InvalidOperationException($"{nameof(BindFilesAsync)}() can only be used with form data.");
        }

        return BindFiles(await request.ReadFormAsync());
    }

    private static FormFile[] BindFiles(IFormCollection kvp)
    {
        var files = kvp.Files.Count > 0 ? new FormFile[kvp.Files.Count] : [];

        for (var i = 0; i < kvp.Files.Count; i++)
        {
            var file = kvp.Files[i];

            if (string.IsNullOrEmpty(file.ContentType))
            {
                throw new InvalidOperationException($"{nameof(BindFiles)}() no content type was set for file '{file.Name}'.");
            }

            if (string.IsNullOrWhiteSpace(Path.GetExtension(file.FileName)))
            {
                throw new InvalidOperationException($"{nameof(BindFiles)}() the file '{file.FileName}' has no extension.");
            }

            files[i] = new()
            {
                Name = file.FileName,
                Description = file.Name,
                ContentType = file.ContentType,
                Stream = file.OpenReadStream(),
            };
        }

        return files;
    }
}

public sealed class FormData<T>
{
    /// <summary>
    /// The bound data.
    /// </summary>
    public T Data { get; init; } = default!;
    /// <summary>
    /// The list of uploaded files.
    /// </summary>
    public FormFile[] Files { get; init; } = default!;
}

public sealed class FormFile
{
    /// <summary>
    /// The file name.
    /// </summary>
    public string Name { get; init; } = default!;
    /// <summary>
    /// The file description.
    /// </summary>
    public string Description { get; init; } = default!;
    /// <summary>
    /// The content type of the file.
    /// </summary>
    public string ContentType { get; set; } = default!;
    /// <summary>
    /// The file stream.
    /// </summary>
    public Stream Stream { get; init; } = default!;

    /// <summary>
    /// Clones this instance, overriding specified values if non-null.
    /// </summary>
    /// <param name="name">The file name.</param>
    /// <param name="contentType">The content type of the file.</param>
    /// <param name="stream">The file stream.</param>
    /// <returns></returns>
    public FormFile With(string? name = null, string? contentType = null, Stream? stream = null)
        => new()
        {
            Name = name ?? Name,
            Description = Description,
            ContentType = contentType ?? ContentType,
            Stream = stream ?? Stream,
        };

    public override string ToString()
        => $"{{ Name = {Name}, Descr = {Description}, ContentType = {ContentType} }}";
}
