using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Peers.Core;
using Peers.Core.Common;
using Peers.Core.Identity;
using Peers.Core.Payments;
using Peers.Core.Payments.Models;
using Peers.Core.Payments.Providers.ClickPay;
using Peers.Core.Payments.Providers.Moyasar;
using Peers.Core.Payments.Providers.Moyasar.Models;
using Peers.Core.Security.Jwt;
using Peers.Modules.Carts.Domain;
using Peers.Modules.Carts.Services;
using Peers.Modules.Customers.Domain;
using Peers.Modules.Kernel;

namespace Peers.Api.Pages.Payments;

[Authorize(Roles = Roles.Customer)]
[IgnoreAntiforgeryToken]
public class PayModel : PageModel
{
    public const string InitiatorQueryKey = "ik";
    public const string SessionIdQueryKey = "sid";

    private readonly PeersContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly IPaymentProvider _paymentProvider;
    private readonly IPaymentProcessor _paymentProcessor;
    private readonly IIdentityInfo _identity;
    private readonly IMemoryCache _cache;
    private readonly ILogger<PayModel> _log;

    public string? PageScript { get; private set; }

    public PayModel(
        PeersContext context,
        TimeProvider timeProvider,
        IPaymentProvider paymentProvider,
        IPaymentProcessor paymentProcessor,
        IIdentityInfo identity,
        IMemoryCache cache,
        ILogger<PayModel> logger)
    {
        _context = context;
        _timeProvider = timeProvider;
        _paymentProvider = paymentProvider;
        _paymentProcessor = paymentProcessor;
        _identity = identity;
        _cache = cache;
        _log = logger;
    }

    public async Task<IActionResult> OnGetAsync(string culture, bool? saveCard)
    {
        var tokenId = GenerateAndCacheTokenId();

        try
        {
            if (await InitiateHostedPagePaymentAsync(culture, tokenId, Request.GetQueryValue(SessionIdQueryKey), saveCard ?? false) is not { } response)
            {
                return RedirectToPage("Result", new { bti = tokenId, paymentProcessFailed = true });
            }

            if (response.Script is not null)
            {
                PageScript = response.Script;
                return Page();
            }
            else if (response.RedirectUrl is not null)
            {
                return Redirect(response.RedirectUrl.ToString());
            }
            else
            {
                _log.UnexpectedPaymentProviderHostedPageInitResponse();
                return RedirectToPage("Result", new { bti = tokenId, paymentProcessFailed = true });
            }
        }
        catch (PaymentProviderException ex)
        {
            _log.PaymentProviderHostedPageInitFailed(ex);
            return RedirectToPage("Result", new { bti = tokenId, paymentProcessFailed = true });
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        using var reader = new StreamReader(Request.Body);
        var json = await reader.ReadToEndAsync();

        if (Request.Query.TryGetValue(InitiatorQueryKey, out var initiatorQueryValue) &&
            initiatorQueryValue.ToString() is { Length: > 0 } initiator)
        {
            var hasSessionId = Request.Query.TryGetValue(SessionIdQueryKey, out _);

            if (initiator is MoyasarPaymentProvider.Name &&
                JsonSerializer.Deserialize(json, MoyasarJsonSourceGenContext.Default.MoyasarPaymentResponse) is { } moyasarResponse &&
                !string.IsNullOrEmpty(moyasarResponse.Id))
            {
                var data = moyasarResponse;
                if (data.Status is MoyasarPaymentResponse.StatusInitiated &&
                    data.Source.Token is not null)
                {
                    var customer = await GetCustomer();
                    // Note, in the return URL (/payments/result) we will check if the operation actually succeeded and if so,
                    // fetch the token and fill in the missing details. Otherwise, we will remove the payment method from the customer.
                    customer.AddPaymentCard(data.Id, null, null, data.Source.Number, null, data.Source.Token, _timeProvider.UtcNow());
                }

                string? orderId = null;
                data.Metadata?.TryGetValue(PaymentInfo.OrderIdKey, out orderId);

                if (hasSessionId &&
                    await GetCheckoutSession(orderId, false) is { } session)
                {
                    session.MarkPayInProgress(data.Id, _timeProvider.UtcNow());
                }

                await _context.SaveChangesAsync();
                return new OkResult();
            }
            else if (initiator is ClickPayPaymentProvider.Name &&
                     JsonSerializer.Deserialize(json, ClickPayJsonSourceGenContext.Default.ClickPayHostedPageCallbackResponse) is { } clickpayResponse &&
                     !string.IsNullOrEmpty(clickpayResponse.TranRef))
            {
                var data = clickpayResponse;
                if (data.PaymentResult.ResponseStatus == "A" &&
                    data.Token is not null)
                {
                    var customer = await GetCustomer();

                    var cardInfo = data.PaymentInfo;
                    var brand = PaymentCardUtils.ResolveCardBrand(cardInfo.CardScheme);
                    var funding = PaymentCardUtils.ResolveCardFunding(cardInfo.CardType);
                    var expiryDate = PaymentCardUtils.GetExpiryDate(cardInfo.ExpiryYear, cardInfo.ExpiryMonth);
                    customer.AddPaymentCard(data.TranRef, brand, funding, cardInfo.PaymentDescription, expiryDate, data.Token, _timeProvider.UtcNow());
                }

                if (hasSessionId &&
                    await GetCheckoutSession(data.CartId, false) is { } session)
                {
                    session.MarkPayInProgress(data.TranRef, _timeProvider.UtcNow());
                }

                await _context.SaveChangesAsync();
                await _paymentProcessor.HandleAsync(data.TranRef!, hasSessionId ? data.CartId : null);

                return new OkResult();
            }
        }
        else
        {
            _log.UnknownPaymentRequestInitiator(initiatorQueryValue);
        }

        return BadRequest();
    }

    private async Task<HostedPagePaymentInitResponse?> InitiateHostedPagePaymentAsync(string culture, string tokenId, string? sessionId, bool saveCard)
    {
        var customer = await _context
            .Users
            .AsNoTracking()
            .FirstAsync(c => c.Id == _identity.Id);

        var returnUrl = BuildLocalUrl(path: "/payments/result", tokenId, sessionId);
        var callbackUrl = BuildLocalUrl(path: "/payments/pay", tokenId, sessionId);
        var customerPhone = customer.PhoneNumber!;
        var customerEmail = customer.Email ?? "user@peers.com.sa";

        if (sessionId is not null)
        {
            if (await GetCheckoutSession(sessionId, true) is { } session)
            {
                var pi = PaymentInfo.ForHpp(session.OrderTotal, session.SessionId.ToString(), $"HPP for {session.SessionId}", customerPhone, customerEmail);

                return await _paymentProvider.InitiateHostedPagePaymentAsync(
                    returnUrl: returnUrl,
                    callbackUrl: callbackUrl,
                    paymentInfo: pi,
                    authOnly: true,
                    tokenize: saveCard,
                    language: culture);
            }
            else
            {
                _log.CheckoutSessionNotFound(sessionId, _identity.Id);
                return null;
            }
        }
        else
        {
            var pi = PaymentInfo.ForTokenization(_identity.Id, customerPhone, customerEmail);
            return await _paymentProvider.InitiateHostedPageTokenizationAsync(
                returnUrl: returnUrl,
                callbackUrl: callbackUrl,
                paymentInfo: pi,
                language: culture);
        }
    }

    private string GenerateAndCacheTokenId()
    {
        var key = TokenIdResolver.GenerateTokenIdCacheKey(out var tokenId);
        var jwt = HttpContext.Request.Headers.Authorization.ToString()[7..];
        _cache.Set(key, jwt, CheckoutSession.HppCheckoutSessionDuration);
        return tokenId;
    }

    private Uri BuildLocalUrl(string path, string tokenId, string? sessionId)
    {
        var request = HttpContext.Request;

        // Check for forwarded headers which is used by devtunnels in debug-only mode.

        var scheme = request.Headers.TryGetValue("x-forwarded-proto", out var fScheme)
            ? fScheme.ToString()
            : request.Scheme;

        var host = request.Headers.TryGetValue("x-forwarded-host", out var fHost)
            ? new HostString(fHost.ToString())
            : request.Host;

        var qs = QueryString.Create(new Dictionary<string, string?>
        {
            [InitiatorQueryKey] = _paymentProvider.ProviderName,
            [TokenIdResolver.TokenIdQueryKey] = tokenId,
        });

        if (sessionId is not null)
        {
            qs = qs.Add(SessionIdQueryKey, sessionId);
        }

        var absoluteUrl = UriHelper.BuildAbsolute(
            scheme,
            host,
            request.PathBase,
            new PathString(path),
            qs
        );

        return new Uri(absoluteUrl);
    }

    private Task<Customer> GetCustomer() => _context
        .Customers
        .Include(p => p.PaymentMethods)
        .FirstAsync(c => c.Id == _identity.Id);

    private async Task<CheckoutSession?> GetCheckoutSession(string? sessionId, bool noTracking)
    {
        var q = noTracking
            ? _context.CheckoutSessions.Include(p => p.Lines).AsNoTracking()
            : _context.CheckoutSessions.AsQueryable();

        return Guid.TryParse(sessionId, out var parsedSessionId)
            ? await q.FirstOrDefaultAsync(p =>
                p.Status == CheckoutSessionStatus.IntentIssued &&
                p.CustomerId == _identity.Id &&
                p.SessionId == parsedSessionId)
            : null;
    }
}
