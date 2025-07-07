using System.Diagnostics;

namespace Mashkoor.Core.Common;

public sealed class TaskTimer : IDisposable
{
    private readonly ILogger _log;
    private readonly string? _name;
    private readonly long _timestamp;

    public TaskTimer(ILogger log, string? name)
    {
        _log = log;
        _name = name;
        _timestamp = Stopwatch.GetTimestamp();
        if (name is not null)
        {
            _log.StartupTaskStarted(name);
        }
        else
        {
            _log.StartupTasksStarting();
        }
    }

    public void Dispose()
    {
        var elapsedMs = Stopwatch.GetElapsedTime(_timestamp).TotalMilliseconds;
        if (_name is not null)
        {
            _log.StartupTaskFinished(_name, elapsedMs);
        }
        else
        {
            _log.StartupTasksDone(elapsedMs);
        }
    }
}
