namespace Peers.Modules.Kernel;

[ExcludeFromCodeCoverage]
internal static partial class LoggerExtensions
{
    [LoggerMessage(LogLevel.Error, "The authenticated user {Username} was not found in the database.", SkipEnabledCheck = true)]
    public static partial void AuthenticatedUserNotFound(this ILogger logger, string? username);

    [LoggerMessage(LogLevel.Warning, "The banned user {Username} is attempting to execute {CommandType}.", SkipEnabledCheck = true)]
    public static partial void BannedUserActivity(this ILogger logger, string? username, Type commandType);
}
