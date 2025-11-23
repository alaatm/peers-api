using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Peers.Core;
using Peers.Core.Identity;
using Peers.Core.Payments;
using Peers.Core.Payments.Providers.ClickPay;
using Peers.Core.Payments.Providers.ClickPay.Models;
using Peers.Core.Payments.Providers.Moyasar;
using Peers.Core.Payments.Providers.Moyasar.Models;
using Peers.Core.Security.Jwt;
using Peers.Modules.Kernel;

namespace Peers.Api.Pages.Payments;

[Authorize(Roles = Roles.Customer)]
[IgnoreAntiforgeryToken]
public class TokenizeModel : PageModel
{
    public const string InitiatorQueryKey = "initiator";

    private readonly TimeProvider _timeProvider;
    private readonly IPaymentProvider _paymentProvider;
    private readonly IIdentityInfo _identity;
    private readonly IMemoryCache _cache;
    private readonly ILogger<TokenizeModel> _log;

    public string? PageScript { get; private set; }

    public TokenizeModel(
        TimeProvider timeProvider,
        IPaymentProvider paymentProvider,
        IIdentityInfo identity,
        IMemoryCache cache,
        ILogger<TokenizeModel> logger)
    {
        _timeProvider = timeProvider;
        _paymentProvider = paymentProvider;
        _identity = identity;
        _cache = cache;
        _log = logger;
    }

    public async Task<IActionResult> OnGetAsync(string culture)
    {
        var tokenId = GenerateAndCacheTokenId();
        var returnUrl = BuildLocalUrl(path: "/payments/result", tokenId);
        var callbackUrl = BuildLocalUrl(path: "/payments/tokenize", tokenId);
        var customerPhone = _identity.Username!;
        var customerEmail = "";

        try
        {
            var response = await _paymentProvider.InitiateHostedPageTokenizationAsync(
                returnUrl, callbackUrl, culture, customerPhone, customerEmail);

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
                return RedirectToPage("Result", new { tokenId, paymentProcessFailed = true });
            }
        }
        catch (PaymentProviderException ex)
        {
            _log.PaymentProviderHostedPageInitFailed(ex);
            return RedirectToPage("Result", new { tokenId, paymentProcessFailed = true });
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        using var reader = new StreamReader(Request.Body);
        var json = await reader.ReadToEndAsync();

        var context = HttpContext.RequestServices.GetRequiredService<PeersContext>();
        var customer = await context
            .Customers
            .Include(p => p.PaymentMethods)
            .FirstAsync(c => c.Id == _identity.Id);

        if (Request.Query.TryGetValue(InitiatorQueryKey, out var initiatorQueryValue) &&
            initiatorQueryValue.ToString() is { Length: > 0 } initiator)
        {
            if (initiator == MoyasarPaymentProvider.Name)
            {
                if (JsonSerializer.Deserialize(json, MoyasarJsonSourceGenContext.Default.MoyasarPaymentResponse) is MoyasarPaymentResponse data &&
                    data.Id is not null &&
                    data.Source?.Number is not null &&
                    data.Source?.Token is not null)
                {
                    // Note, in the return URL (/payments/result) we will check if the operation actually succeeded and if so,
                    // fetch the token and fill in the missing details. Otherwise, we will remove the payment method from the customer.
                    customer.AddPaymentCard(data.Id, null, null, data.Source.Number, null, data.Source.Token, _timeProvider.UtcNow());
                    await context.SaveChangesAsync();
                    return new CreatedResult();
                }
            }
            else if (initiator == ClickPayPaymentProvider.Name)
            {
                var data = JsonSerializer.Deserialize(json, ClickPayJsonSourceGenContext.Default.ClickPayHostedPageCallbackResponse);
                if (data?.PaymentResult.ResponseStatus == "A")
                {
                    var cardInfo = data.PaymentInfo;
                    var brand = PaymentCardUtils.ResolveCardBrand(cardInfo.CardScheme);
                    var funding = PaymentCardUtils.ResolveCardFunding(cardInfo.CardType);
                    var expiryDate = PaymentCardUtils.GetExpiryDate(cardInfo.ExpiryYear, cardInfo.ExpiryMonth);
                    customer.AddPaymentCard(data.TranRef, brand, funding, cardInfo.PaymentDescription, expiryDate, data.Token, _timeProvider.UtcNow());
                    await context.SaveChangesAsync();
                }

                return new CreatedResult();
            }
        }
        else
        {
            _log.UnknownPaymentRequestInitiator(initiatorQueryValue);
        }

        return BadRequest();
    }

    private string GenerateAndCacheTokenId()
    {
        var key = TokenIdResolver.GenerateTokenIdCacheKey(out var tokenId);
        var jwt = HttpContext.Request.Headers.Authorization.ToString()[7..];
        _cache.Set(key, jwt, TimeSpan.FromMinutes(15));
        return tokenId;
    }

    private Uri BuildLocalUrl(string path, string tokenId)
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

        var absoluteUrl = UriHelper.BuildAbsolute(
            scheme,
            host,
            request.PathBase,
            new PathString(path),
            qs
        );

        return new Uri(absoluteUrl);
    }
}
