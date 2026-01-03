using Peers.Core.Payments;
using Peers.Core.Payments.Models;
using Peers.Modules;
using Peers.Modules.Carts.Domain;
using Peers.Modules.Customers.Domain;
using Peers.Modules.Ordering.Domain;

namespace Peers.Modules.Carts.Services;

public sealed class PaymentProcessor : IPaymentProcessor
{
    private readonly PeersContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly IPaymentProvider _paymentProvider;
    private readonly IIdentityInfo _identity;
    private readonly ILogger<PaymentProcessor> _logger;

    public PaymentProcessor(
        PeersContext context,
        TimeProvider timeProvider,
        IPaymentProvider paymentProvider,
        IIdentityInfo identity,
        ILogger<PaymentProcessor> logger)
    {
        _context = context;
        _timeProvider = timeProvider;
        _paymentProvider = paymentProvider;
        _identity = identity;
        _logger = logger;
    }

    public async Task HandleAsync(string paymentId, string? sessionId)
    {
        if (await _paymentProvider.FetchPaymentAsync(paymentId) is { } payment)
        {
            if (sessionId is null)
            {
                await HandleCardTokenizationPaymentAsync(payment);
            }
            else if (Guid.TryParse(sessionId, out var parsedSessionId))
            {
                await HandleSessionPaymentAsync(payment, parsedSessionId);
            }
            else
            {
                _logger.InvalidCheckoutSession(sessionId);
            }
        }

        _logger.InvalidGatewayPaymentId(paymentId);
    }

    private async Task HandleSessionPaymentAsync(PaymentResponse payment, Guid sessionId)
    {
        var (customer, tokenizedCard) = await GetCustomerAndPaymentCardAsync(payment);

        if (await _context
            .CheckoutSessions
            .Include(p => p.Cart).ThenInclude(p => p.Buyer)
            .Include(p => p.Cart).ThenInclude(p => p.Seller)
            .Include(p => p.Lines).ThenInclude(p => p.Variant)
            .FirstOrDefaultAsync(p =>
                p.Status == CheckoutSessionStatus.Paying &&
                p.CustomerId == _identity.Id &&
                p.SessionId == sessionId &&
                p.PaymentId == payment.PaymentId) is { } session)
        {
            if (payment.IsSuccessful)
            {
                session.MarkCompleted(payment, _timeProvider.UtcNow());
                await ActivateCustomerPaymentCardIfExistsAsync(customer, tokenizedCard);
            }
            else
            {
                session.MarkFailed(_timeProvider.UtcNow());
            }

            await _context.SaveChangesAsync();
        }

        _logger.CheckoutSessionNotFound(sessionId);
    }

    private async Task HandleCardTokenizationPaymentAsync(PaymentResponse payment)
    {
        var (customer, tokenizedCard) = await GetCustomerAndPaymentCardAsync(payment);

        if (payment.IsSuccessful)
        {
            var pi = PaymentInfo.ForTransactionApi(payment.Amount, $"{customer.Id}", "Void tokenization payment");
            await _paymentProvider.VoidOrRefundPaymentAsync(payment.PaymentId, pi);
            await ActivateCustomerPaymentCardIfExistsAsync(customer, tokenizedCard);
        }
        else
        {
            if (tokenizedCard is not null)
            {
                customer.DeletePaymentCard(tokenizedCard, _timeProvider.UtcNow(), force: true);
            }
        }

        await _context.SaveChangesAsync();
    }

    private async Task<(Customer, PaymentCard?)> GetCustomerAndPaymentCardAsync(PaymentResponse payment)
    {
        var customer = await _context
            .Customers
            .Include(p => p.PaymentMethods.Where(p => !p.IsDeleted))
            .FirstAsync(c => c.Id == _identity.Id);

        var tokenizedCard = customer
            .PaymentMethods
            .Where(p => p.Type == PaymentType.Card)
            .Cast<PaymentCard>()
            .FirstOrDefault(p => p.PaymentId == payment.PaymentId);

        return (customer, tokenizedCard);
    }

    private async Task ActivateCustomerPaymentCardIfExistsAsync(Customer customer, PaymentCard? card)
    {
        if (card is not null &&
            await _paymentProvider.FetchTokenAsync(card.Token) is { } tokenResponse)
        {
            customer.ActivatePaymentCard(
                card,
                tokenResponse.CardBrand,
                tokenResponse.CardType,
                tokenResponse.Expiry,
                _timeProvider.UtcToday());
        }
    }
}
