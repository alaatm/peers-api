using Peers.Core.Payments;
using Peers.Core.Payments.Models;
using Peers.Modules.Carts.Domain;
using Peers.Modules.Carts.Events;
using Peers.Modules.Carts.Services;
using Peers.Modules.Customers.Domain;

namespace Peers.Modules.Carts.EventHandlers;

public sealed class OnApiPaymentIntentIssued : INotificationHandler<ApiPaymentIntentIssued>
{
    private readonly PeersContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly IPaymentProvider _paymentProvider;
    private readonly IPaymentProcessor _paymentProcessor;
    private readonly ILogger<OnApiPaymentIntentIssued> _logger;

    public OnApiPaymentIntentIssued(
        PeersContext context,
        TimeProvider timeProvider,
        IPaymentProvider paymentProvider,
        IPaymentProcessor paymentProcessor,
        ILogger<OnApiPaymentIntentIssued> logger)
    {
        _context = context;
        _timeProvider = timeProvider;
        _paymentProvider = paymentProvider;
        _paymentProcessor = paymentProcessor;
        _logger = logger;
    }

    public async Task Handle([NotNull] ApiPaymentIntentIssued notification, CancellationToken ctk)
    {
        var session = await _context
            .CheckoutSessions
            .FirstOrDefaultAsync(p =>
                p.Status == CheckoutSessionStatus.IntentIssued &&
                p.PaymentType == CheckoutSessionPaymentType.Api &&
                p.CustomerId == notification.UserId &&
                p.SessionId == notification.SessionId, ctk);

        if (session is null)
        {
            _logger.CheckoutSessionNotFound(notification.SessionId);
            return;
        }

        var now = _timeProvider.UtcNow();
        var card = (PaymentCard)session.PaymentMethod!;
        var sessionId = session.SessionId.ToString();

        try
        {
            var pi = PaymentInfo.ForTransactionApi(session.OrderTotal, sessionId, $"Payment for {sessionId}");
            var response = await _paymentProvider.AuthorizePaymentAsync(PaymentSourceType.TokenizedCard, card.Token, pi);
            session.MarkPayInProgress(response.PaymentId, now);
            await _context.SaveChangesAsync(ctk);
            await _paymentProcessor.HandleAsync(response.PaymentId, sessionId);
        }
        catch (PaymentProviderException ex)
        {
            _logger.PaymentOperationFailed(ex, "authorize", sessionId);
            session.MarkFailed(now);
            await _context.SaveChangesAsync(ctk);
        }
    }
}
