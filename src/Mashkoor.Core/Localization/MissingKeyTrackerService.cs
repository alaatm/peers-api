namespace Mashkoor.Core.Localization;

public interface IMissingKeyTrackerService
{
    /// <summary>
    /// Logs a missing key for the given language.
    /// </summary>
    /// <param name="key">The resource key.</param>
    /// <param name="language">The language.</param>
    void TrackMissingKey(string key, string? language);
}

/// <summary>
/// Missing key tracker service that logs missing keys to file system.
/// </summary>
public sealed class MissingKeyTrackerService : IMissingKeyTrackerService, IDisposable
{
    private static readonly SemaphoreSlim _semaphore = new(1, 1);

    private readonly Dictionary<string, HashSet<string>> _missingKeysByLanguage = [];
    private readonly PeriodicTimer _timer = new(TimeSpan.FromSeconds(10));
    private readonly string _missingKeysDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Mashkoor");

    public IReadOnlyDictionary<string, HashSet<string>> MissingKeysByLanguage => _missingKeysByLanguage;

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
    public MissingKeyTrackerService() => Initialize();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

    /// <summary>
    /// Logs a missing key for the given language.
    /// </summary>
    /// <param name="key">The resource key.</param>
    /// <param name="language">The language.</param>
    public void TrackMissingKey(string key, string? language)
    {
        _semaphore.Wait();

        try
        {
            if (language?.ToUpperInvariant() == "EN")
            {
                return;
            }

            language ??= "Unknown";

            if (!_missingKeysByLanguage.TryGetValue(language, out var value))
            {
                value = [];
                _missingKeysByLanguage[language] = value;
            }

            value.Add(key);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task Initialize()
    {
        Directory.CreateDirectory(_missingKeysDir);

        while (await _timer.WaitForNextTickAsync())
        {
            await _semaphore.WaitAsync();

            try
            {
                foreach (var key in _missingKeysByLanguage.Keys)
                {
                    var path = Path.Combine(_missingKeysDir, $"{key}.txt");

                    if (File.Exists(path))
                    {
                        var existing = await File.ReadAllLinesAsync(path);
                        var newEntries = _missingKeysByLanguage[key].Except(existing);
                        if (newEntries.Any())
                        {
                            await File.AppendAllLinesAsync(path, newEntries);
                        }
                    }
                    else
                    {
                        await File.WriteAllLinesAsync(path, _missingKeysByLanguage[key]);
                    }
                }

                _missingKeysByLanguage.Clear();
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }

    public void Dispose() => _timer.Dispose();
}
