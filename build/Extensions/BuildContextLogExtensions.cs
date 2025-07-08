using Cake.Core;
using Cake.Core.Diagnostics;

namespace Build.Extensions;

public static partial class BuildContextExtensions
{
    public static void LogCritical(this BuildContext context, string message)
        => context.Log.Write(Verbosity.Normal, LogLevel.Fatal, message);

    public static void LogError(this BuildContext context, string message)
        => context.Log.Write(Verbosity.Normal, LogLevel.Error, message);

    public static void LogWarning(this BuildContext context, string message)
        => context.Log.Write(Verbosity.Normal, LogLevel.Warning, message);

    public static void LogWarning(this BuildContext context, string message, int padLen)
    {
        var paddedMessage = new string(' ', padLen) + message;
        context.Log.Write(Verbosity.Normal, LogLevel.Warning, paddedMessage);
    }

    public static void LogInformation(this BuildContext context, string message)
        => context.Log.Write(Verbosity.Normal, LogLevel.Information, message);

    public static void LogInformation(this BuildContext context, string message, int padLen)
    {
        var paddedMessage = new string(' ', padLen) + message;
        context.Log.Write(Verbosity.Normal, LogLevel.Information, paddedMessage);
    }

    public static void LogDebug(this BuildContext context, string message)
        => context.Log.Write(Verbosity.Diagnostic, LogLevel.Debug, message);

    public static void LogTrace(this BuildContext context, string message)
        => context.Log.Write(Verbosity.Diagnostic, LogLevel.Verbose, message);
}

public static class CakeContextExtensions
{
    public static void LogCritical(this ICakeContext context, string message)
        => context.Log.Write(Verbosity.Normal, LogLevel.Fatal, message);

    public static void LogError(this ICakeContext context, string message)
        => context.Log.Write(Verbosity.Normal, LogLevel.Error, message);

    public static void LogWarning(this ICakeContext context, string message)
        => context.Log.Write(Verbosity.Normal, LogLevel.Warning, message);

    public static void LogInformation(this ICakeContext context, string message)
        => context.Log.Write(Verbosity.Normal, LogLevel.Information, message);

    public static void LogDebug(this ICakeContext context, string message)
        => context.Log.Write(Verbosity.Verbose, LogLevel.Debug, message);
}
