using System.Diagnostics.CodeAnalysis;

namespace Peers.Api.Pages.Payments;

[ExcludeFromCodeCoverage]
internal static partial class LoggerExtensions
{
    [LoggerMessage(LogLevel.Error, "Payment provider hosted page init failed", SkipEnabledCheck = true)]
    public static partial void PaymentProviderHostedPageInitFailed(this ILogger logger, Exception ex);

    [LoggerMessage(LogLevel.Warning, "Received unexpected response from hosted page init func", SkipEnabledCheck = true)]
    public static partial void UnexpectedPaymentProviderHostedPageInitResponse(this ILogger logger);

    [LoggerMessage(LogLevel.Warning, "Unknown payment request callback initiator: {Initiator}", SkipEnabledCheck = true)]
    public static partial void UnknownPaymentRequestInitiator(this ILogger logger, string? initiator);
}
