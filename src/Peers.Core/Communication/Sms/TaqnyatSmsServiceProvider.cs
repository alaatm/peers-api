using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using Microsoft.Net.Http.Headers;
using Peers.Core.Common;
using Peers.Core.Communication.Sms.Configuration;

namespace Peers.Core.Communication.Sms;

public sealed class TaqnyatSmsServiceProvider : ISmsServiceProvider
{
    private readonly HttpClient _httpClient;
    private readonly SmsConfig _config;
    private readonly ILogger<TaqnyatSmsServiceProvider> _log;

    public TaqnyatSmsServiceProvider(
        [NotNull] HttpClient httpClient,
        [NotNull] SmsConfig config,
        ILogger<TaqnyatSmsServiceProvider> log)
    {
        _log = log;
        _config = config;
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://api.taqnyat.sa/");
        _httpClient.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", config.Key);
    }

    /// <summary>
    /// Asynchronously sends an SMS to a recipient.
    /// </summary>
    /// <param name="recipient">The recipient.</param>
    /// <param name="body">The message.</param>
    public async Task<TaqnyatResponse?> SendAsync(
        [NotNull] string recipient,
        [NotNull] string body)
    {
        _log.SmsSendRequest(recipient);
        var payload = new
        {
            _config.Sender,
            Recipients = new[] { recipient.Replace("+", "", StringComparison.Ordinal) },
            Body = body,
        };

        var response = await _httpClient.PostAsJsonAsync("v1/messages", payload, options: GlobalJsonOptions.Default);
        if (response.IsSuccessStatusCode)
        {
            _log.SmsSendRequestSuccess(recipient);
            return await response.Content.ReadFromJsonAsync<TaqnyatResponse>();
        }

        var errorResponse = await response.Content.ReadFromJsonAsync<TaqnyatErrorResponse>();
        _log.SmsSendRequestFail(response.StatusCode, errorResponse?.Message);
        return null;
    }
}

[ExcludeFromCodeCoverage]
public sealed class TaqnyatResponse
{
    public int StatusCode { get; set; }
    public long MessageId { get; set; }
    public string Cost { get; set; } = default!;
    public string Currency { get; set; } = default!;
    public int TotalCount { get; set; }
    public int MsgLength { get; set; }
    public string Accepted { get; set; } = default!;
    public string Rejected { get; set; } = default!;
}

[ExcludeFromCodeCoverage]
public sealed class TaqnyatErrorResponse
{
    public int StatusCode { get; set; }
    public string? Message { get; set; }
}
