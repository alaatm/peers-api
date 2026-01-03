namespace Peers.Modules.Carts;

[ExcludeFromCodeCoverage]
internal static partial class LoggerExtensions
{
    [LoggerMessage(LogLevel.Warning, "Concurrency conflict when updating cart for buyer {BuyerId} and seller {SellerId}.", SkipEnabledCheck = true)]
    public static partial void CartMutationOperationConcurrency(this ILogger logger, Exception ex, int buyerId, int sellerId);

    [LoggerMessage(LogLevel.Error, "Checkout session {SessionId} payment {Operation} failed.", SkipEnabledCheck = true)]
    public static partial void PaymentOperationFailed(this ILogger logger, Exception ex, string operation, string sessionId);

    [LoggerMessage(LogLevel.Warning, "Checkout session {SessionId} not found.", SkipEnabledCheck = true)]
    public static partial void CheckoutSessionNotFound(this ILogger logger, Guid sessionId);

    [LoggerMessage(LogLevel.Warning, "Invalid Checkout session {SessionId}.", SkipEnabledCheck = true)]
    public static partial void InvalidCheckoutSession(this ILogger logger, string sessionId);

    [LoggerMessage(LogLevel.Warning, "Invalid gateway payment id {PaymentId}.", SkipEnabledCheck = true)]
    public static partial void InvalidGatewayPaymentId(this ILogger logger, string paymentId);
}
