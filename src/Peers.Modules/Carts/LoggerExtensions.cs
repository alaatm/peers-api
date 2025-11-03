namespace Peers.Modules.Carts;

[ExcludeFromCodeCoverage]
internal static partial class LoggerExtensions
{
    [LoggerMessage(LogLevel.Warning, "Concurrency conflict when updating cart for buyer {BuyerId} and seller {SellerId}.", SkipEnabledCheck = true)]
    public static partial void CartMutationOperationConcurrency(this ILogger logger, Exception ex, int buyerId, int sellerId);
}
