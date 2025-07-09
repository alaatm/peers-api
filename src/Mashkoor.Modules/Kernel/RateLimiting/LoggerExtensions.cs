namespace Mashkoor.Modules.Kernel.RateLimiting;

[ExcludeFromCodeCoverage]
internal static partial class LoggerExtensions
{
    [LoggerMessage(LogLevel.Warning, "Rate limit rejected for path: {Path}.", SkipEnabledCheck = true)]
    public static partial void RateLimitRejected(this ILogger logger, string path);
}
