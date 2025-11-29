namespace Peers.Modules.Sellers;

[ExcludeFromCodeCoverage]
internal static partial class LoggerExtensions
{
    [LoggerMessage(LogLevel.Error, "Nafath enrollment request failed for user {UserId} with national ID {NationalId}.", SkipEnabledCheck = true)]
    public static partial void NafathRequestError(this ILogger logger, Exception ex, int userId, string nationalId);
}
