using System.Buffers;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Peers.Core.Common;
using Peers.Core.Payments.Providers.ClickPay;
using Peers.Core.Payments.Providers.ClickPay.Configuration;
using Peers.Core.Payments.Providers.Moyasar.Models;
using Peers.Modules.Carts.Services;
using Peers.Modules.Kernel;

namespace Peers.Api.Pages.Payments;

[Authorize(Roles = Roles.Customer)]
[IgnoreAntiforgeryToken]
public class ResultModel : PageModel
{
    private readonly IMemoryCache _cache;
    private readonly IPaymentProcessor _paymentProcessor;

    public bool Success { get; private set; }
    public string? Message { get; private set; }

    public ResultModel(
        IMemoryCache cache,
        IPaymentProcessor paymentProcessor)
    {
        _cache = cache;
        _paymentProcessor = paymentProcessor;
    }

    // This route, along with the POST below are used by mobile clients to receive the payment status.
    // This route can be called from 2 different sources:
    //
    // 1. Moyasar (GET):
    //    As the final callback to indicate the success or failure of the payment.
    //    This must be used to determine whether to keep the saved tokenized card (which was received and saved in POST /payments/pay) or not.
    //    Possible failures include but not limited to failed 3D Secure authentication, etc.
    //
    // 2. A redirect from /payments/tokenization (GET):
    //    As a failure of the initial ClickPay request.
    //    As a failure of the initial Moyasar request.
    //
    public async Task<PageResult> OnGetAsync(string? id, string? status, string? amount, string? message, bool? paymentProcessFailed)
    {
        if (paymentProcessFailed == true)
        {
            Success = false;
            Message = "Internal Server Error";
        }
        else
        {
            Success = status is not MoyasarPaymentResponse.StatusFailed;
            Message = message;
        }

        if (id is not null)
        {
            await _paymentProcessor.HandleAsync(id, Request.GetQueryValue(PayModel.SessionIdQueryKey));
        }

        return Page();
    }

    // This route is called from ClickPay (POST):
    // It checks the authenticity of the request check and setting values for client apps to receive notification on the payment status.
    // The actual payment status is determined by the POST /payments/pay.
    public async Task<IActionResult> OnPostAsync()
    {
        if (!IsValidClickPaySignature())
        {
            return BadRequest("Invalid signature");
        }

        var form = Request.Form;
        var respMessage = form["respMessage"];
        var respStatus = form["respStatus"];

        Success = respStatus == "A";
        Message = respMessage;

        return Page();
    }

    private bool IsValidClickPaySignature()
    {
        const string SignatureKey = "signature";

        if (!string.Equals(Request.GetQueryValue(PayModel.InitiatorQueryKey), ClickPayPaymentProvider.Name, StringComparison.OrdinalIgnoreCase))
        {
            // Not a ClickPay request, nothing to validate
            return true;
        }

        var form = Request.Form;
        if (form[SignatureKey].ToString() is not { Length: > 0 } signature)
        {
            return false;
        }

        var config = Request.HttpContext.RequestServices.GetRequiredService<ClickPayConfig>();
        var serverKey = config.Key;

        // 1. Remove any empty values and the signature pair
        // 2. Sort the params by key
        // 3. Url encode the params (key and value)
        // 4. Calculate the HMAC SHA256 hash of the params using the server key

        var elements = form
            .Where(p =>
                !string.IsNullOrEmpty(p.Value) &&
                !p.Key.Equals(SignatureKey, StringComparison.OrdinalIgnoreCase))
            .OrderBy(p => p.Key, StringComparer.Ordinal)
            // Flatten multiple values to guard against someone sending multiple form entries with the same key.
            .SelectMany(p => p.Value, (p, v) => (p.Key, Value: v))
            .Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value!)}");

        var strToSign = string.Join('&', elements);

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(serverKey));
        var computedBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(strToSign));

        Span<byte> incomingBytes = stackalloc byte[computedBytes.Length];
        if (Convert.FromHexString(signature, incomingBytes, out var charsConsumed, out var bytesWritten) is not OperationStatus.Done ||
            charsConsumed != signature.Length ||
            bytesWritten != computedBytes.Length)
        {
            // Bad hex, wrong length, or extra characters
            return false;
        }

        // Constant-time compare to prevents timing attacks
        return CryptographicOperations.FixedTimeEquals(computedBytes, incomingBytes);
    }
}
