using System.Diagnostics;
using System.Globalization;
using Peers.Core.Background;
using Peers.Core.Cqrs.Pipeline;
using Peers.Modules;
using Peers.Modules.Carts.Domain;
using Peers.Modules.Carts.Events;
using Peers.Modules.Carts.Queries;
using Peers.Modules.Carts.Services;
using Peers.Modules.Customers.Domain;

namespace Peers.Modules.Carts.Commands;

public static class Pay
{
    /// <summary>
    /// Submits the shopping cart for the current buyer and specified seller for payment processing.
    /// </summary>
    /// <param name="SellerId">The unique identifier of the seller.</param>
    /// <param name="CustomerAddressId">The unique identifier of the customer's address to use for shipping calculations.</param>
    /// <param name="PaymentMethodId">The unique identifier of the saved payment method to use. If null, a new card payment method will be used.</param>
    /// <param name="SaveCard">Indicates whether to save the card details for future use. Applicable only when using a new card payment method.</param>
    [Authorize(Roles = Roles.Customer)]
    public sealed record Command(
        int SellerId,
        int CustomerAddressId,
        int? PaymentMethodId,
        bool? SaveCard) : LocalizedCommand, IValidatable;

    /// <summary>
    /// Represents the result of the payment operation. The uri varies depending on the payment method used.
    ///
    /// #### Saved Card Payment Method (Api)
    ///    The uri can be used to track the status of the payment. The payment is being processed in the background.
    /// 
    /// #### Credit/Debit Card Payment Method (HPP)
    ///    The uri should be used to redirect the user to the hosted payment page (HPP) to complete the payment.
    ///    
    /// </summary>
    /// <param name="SessionId">The checkout session id.</param>
    /// <param name="PaymentType">The payment type.</param>
    public sealed record Response(
        string SessionId,
        CheckoutSessionPaymentType PaymentType);

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(p => p.SellerId).GreaterThan(0);
            RuleFor(p => p.CustomerAddressId).GreaterThan(0);
            RuleFor(p => p.PaymentMethodId).GreaterThan(0).When(p => p.PaymentMethodId.HasValue);
        }
    }

    public sealed class Handler : ICommandHandler<Command>
    {
        private readonly PeersContext _context;
        private readonly TimeProvider _timeProvider;
        private readonly LinkGenerator _linkGenerator;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IProducer _producer;
        private readonly IShippingCalculator _shippingCalculator;
        private readonly IIdentityInfo _identity;
        private readonly ILogger<Handler> _logger;
        private readonly IStrLoc _l;

        public Handler(
            PeersContext context,
            TimeProvider timeProvider,
            LinkGenerator linkGenerator,
            IHttpContextAccessor contextAccessor,
            IProducer producer,
            IShippingCalculator shippingCalculator,
            IIdentityInfo identity,
            ILogger<Handler> logger,
            IStrLoc l)
        {
            _context = context;
            _timeProvider = timeProvider;
            _linkGenerator = linkGenerator;
            _contextAccessor = contextAccessor;
            _producer = producer;
            _shippingCalculator = shippingCalculator;
            _identity = identity;
            _logger = logger;
            _l = l;
        }

        public async Task<IResult> Handle([NotNull] Command cmd, CancellationToken ctk)
        {
            try
            {
                CheckoutSession? session = null;

                var result = await WithCartLockAsync(_identity.Id, cmd.SellerId, async () =>
                {
                    var now = _timeProvider.UtcNow();

                    if (await _context.Carts
                        .Include(p => p.Buyer).ThenInclude(p => p.AddressList.Where(p => p.Id == cmd.CustomerAddressId))
                        .Include(p => p.Buyer).ThenInclude(p => p.PaymentMethods.Where(p => p.Id == cmd.PaymentMethodId))
                        .Include(p => p.Lines).ThenInclude(p => p.Listing)
                        .Include(p => p.Lines).ThenInclude(p => p.Variant)
                        .Include(p => p.CheckoutSessions.Where(p =>
                                p.Status == CheckoutSessionStatus.Active ||
                                p.Status == CheckoutSessionStatus.IntentIssued ||
                                p.Status == CheckoutSessionStatus.Paying))
                        .Include(p => p.CheckoutSessions).ThenInclude(p => p.ShippingAddress)
                        .Include(p => p.CheckoutSessions).ThenInclude(p => p.PaymentMethod)
                        .FirstOrDefaultAsync(p =>
                            p.BuyerId == _identity.Id &&
                            p.SellerId == cmd.SellerId, ctk) is not Cart cart)
                    {
                        return Result.BadRequest(_l["Cart does not exist."]);
                    }

                    if (cart.Buyer.AddressList.Find(p => p.Id == cmd.CustomerAddressId) is not { } deliveryAddress)
                    {
                        return Result.BadRequest(_l["The provided customer address does not exist."]);
                    }

                    var paymentMethod = cmd.PaymentMethodId != null
                        ? cart.Buyer.PaymentMethods.Find(p => p.Id == cmd.PaymentMethodId)
                        : null;

                    var sessionResult = await GetOrCreateSession(cart, deliveryAddress, paymentMethod, ctk);
                    session = sessionResult.session;
                    var errors = sessionResult.errors;

                    if (session is null)
                    {
                        return Result.Conflict(_l["Could not create or retrieve an active checkout session. Please try again."]);
                    }

                    if (errors is not null)
                    {
                        return Result.BadRequest(_l["Cart checkout failed."]); // TODO: Pass errors
                    }

                    if (session.Status is CheckoutSessionStatus.Paying)
                    {
                        return Result.Conflict(_l["There is already a payment in progress for this cart. Please wait for it to complete."]);
                    }
                    else if (session.Status is CheckoutSessionStatus.IntentIssued)
                    {
                        return GetAcceptedResult(session, cmd);
                    }

                    // Active session, start payment intent

                    session.MarkIntentIssued(now);
                    await _context.SaveChangesAsync(ctk);

                    return GetAcceptedResult(session, cmd);

                }, ctk);

                if (session is { } &&
                    session.Status is CheckoutSessionStatus.IntentIssued &&
                    session.PaymentMethod is not null)
                {
                    await _producer.PublishAsync(new ApiPaymentIntentIssued(session.SessionId, _identity), ctk);
                }

                return result;
            }
            catch (InvalidOperationException)
            {
                return Result.Conflict(_l["Another operation is updating this cart. Please retry shortly."]);
            }
        }

        private async Task<(CheckoutSession? session, IReadOnlyDictionary<CartLine, string>? errors)> GetOrCreateSession(
            Cart cart,
            CustomerAddress deliveryAddress,
            PaymentMethod? paymentMethod,
            CancellationToken ctk)
        {
            var now = _timeProvider.UtcNow();

            if (cart.CheckoutSessions.SingleOrDefault(p => p.Status is
                CheckoutSessionStatus.Active or
                CheckoutSessionStatus.IntentIssued or
                CheckoutSessionStatus.Paying) is { } currentSession)
            {
                if (currentSession.Status is CheckoutSessionStatus.Paying)
                {
                    // There's already a payment in progress for this cart
                    return (currentSession, null);
                }

                // Active or IntentIssued session exists, check if still valid and update if needed
                Debug.Assert(currentSession.Status is CheckoutSessionStatus.Active or CheckoutSessionStatus.IntentIssued);

                try
                {
                    if (currentSession.IsExpired(now))
                    {
                        currentSession.MarkExpired(now);
                        await _context.SaveChangesAsync(ctk);
                    }
                    else
                    {
                        // Update if delivery address or payment method changed
                        if (currentSession.ShippingAddressId != deliveryAddress.Id ||
                            currentSession.PaymentMethodId != paymentMethod?.Id)
                        {
                            currentSession.Update(deliveryAddress, paymentMethod, now);
                            await _context.SaveChangesAsync(ctk);
                        }

                        return (currentSession, null);
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Could happen if the session was marked expired in another parallel request
                    // or from a background job. Just fail and let the caller try again.
                    return (null, null);
                }
            }

            // No non-terminal session exists or the existing one was expired, create a new one.

            var shippingCalcResult = await _shippingCalculator.CalculateAsync(cart, deliveryAddress.Address.Location, ctk);
            Debug.Assert(shippingCalcResult.Outcome is ShippingCalculationOutcome.Success);
            var shippingFeeTotal = shippingCalcResult.Total;

            if (cart.TryCheckout(
                deliveryAddress,
                paymentMethod,
                shippingFeeTotal,
                now,
                out var session,
                out var errors))
            {
                await _context.SaveChangesAsync(ctk);
                return (session, null);
            }

            return (null, errors);
        }

        private async Task<T> WithCartLockAsync<T>(int buyerId, int sellerId, Func<Task<T>> action, CancellationToken ctk)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(ctk);
            var resource = string.Format(CultureInfo.InvariantCulture, CartMutationOperation.CartOperLockName, buyerId, sellerId);

            if ((await _context.AcquireAppLockAsync(transaction, resource, 10_000, ctk)) is < 0 and var rc)
            {
                throw new InvalidOperationException("Could not acquire cart lock.");
            }

            var result = await action();

            await transaction.CommitAsync(ctk);
            return result;
        }

        private IResult GetAcceptedResult(
            CheckoutSession session,
            Command cmd)
        {
            var sid = session.SessionId.ToString("N");
            var context = _contextAccessor.HttpContext!;
            var opts = new LinkOptions
            {
                AppendTrailingSlash = true,
                LowercaseUrls = true,
            };

            var uri = session.PaymentType switch
            {
                CheckoutSessionPaymentType.HostedPagePayment => _linkGenerator.GetUriByPage(
                    httpContext: context,
                    page: "/payments/pay",
                    values: new { culture = cmd.Lang, saveCard = cmd.SaveCard?.ToString().ToLowerInvariant(), sid },
                    options: opts),
                CheckoutSessionPaymentType.Api => _linkGenerator.GetUriByName(
                    httpContext: context,
                    endpointName: GetCheckoutSessionStatus.EndpointName,
                    values: new { sid },
                    options: opts),
                _ => throw new UnreachableException(),
            };

            return Result.Accepted(uri, new Response(sid, session.PaymentType));
        }
    }
}
