using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Peers.Core.Nafath.Configuration;
using Peers.Core.Nafath.Models;
using Peers.Core.Nafath.Utils;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;

namespace Peers.Core.Nafath;

/// <summary>
/// Provides high-level operations for interacting with the Nafath authentication service, including initiating
/// verification requests, retrieving request statuses, and validating authentication tokens.
/// </summary>
public sealed class NafathService : INafathService
{
    private static readonly SemaphoreSlim _lock = new(1, 1);
    private static readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler = new();
    private static readonly MemoryCacheEntryOptions _requestCacheEntryOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60)
    };

    private const string SandboxApiBaseUrl = "https://nafath.api.elm.sa/stg/";
    private const string ProductionApiBaseUrl = "https://nafath.api.elm.sa/";
    private const string SendRequestEndpoint = "api/v1/mfa/request?local={0}&requestId={1}";
    private const string RetrieveRequestStatusEndpoint = "api/v1/mfa/request/status";
    private const string RetrieveJwkEndpoint = "api/v1/mfa/jwk";

    private readonly HttpClient _httpClient;
    private readonly NafathConfig _config;
    private readonly IMemoryCache _cache;

    /// <summary>
    /// Initializes a new instance of the <see cref="NafathService"/> class.
    /// </summary>
    /// <param name="httpClient">The http client.</param>
    /// <param name="config">The configuration.</param>
    /// <param name="cache">The memory cache.</param>
    public NafathService(
        [NotNull] HttpClient httpClient,
        [NotNull] NafathConfig config,
        IMemoryCache cache)
    {
        var apiBaseUrl = config.UseSandbox ? SandboxApiBaseUrl : ProductionApiBaseUrl;

        _cache = cache;
        _config = config;
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(apiBaseUrl);
        _httpClient.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
        _httpClient.DefaultRequestHeaders.Add("APP-ID", _config.AppId);
        _httpClient.DefaultRequestHeaders.Add("APP-KEY", _config.AppKey);
    }

    /// <summary>
    /// Sends a request to the Nafath service to initiate a verification process.
    /// </summary>
    /// <param name="locale">The locale for the request.</param>
    /// <param name="userId">The user identifier.</param>
    /// <param name="nationalId">The national ID/Iqama number.</param>
    /// <param name="ctk">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="NafathSendRequestResponse"/></returns>
    public async Task<NafathSendRequestResponse> SendRequestAsync(
        string locale,
        int userId,
        string nationalId,
        CancellationToken ctk = default)
    {
        if (_cache.TryGetValue<NafathSendRequestResponse>(nationalId, out var cached))
        {
            return cached!;
        }

        await _lock.WaitAsync(ctk);
        try
        {
            if (_cache.TryGetValue(nationalId, out cached))
            {
                return cached!;
            }

            var response = await SendRequestCoreAsync<NafathSendRequestResponse>(
                HttpMethod.Post,
                Format(SendRequestEndpoint, locale, GuidIntCodec.Encode(userId)),
                new NafathSendRequestRequest(nationalId),
                ctk);

            return _cache.Set(nationalId, response, _requestCacheEntryOptions);
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Asynchronously retrieves the status of a Nafath authentication request for the specified user and transaction.
    /// </summary>
    /// <param name="nationalId">The national ID/Iqama number of the user whose request status is being retrieved.</param>
    /// <param name="transactionId">The unique identifier of the Nafath transaction associated with the authentication request.</param>
    /// <param name="random">A random string used to correlate or validate the request.</param>
    /// <param name="ctk">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="NafathRequestStatus"/></returns>
    public async Task<NafathRequestStatus> RetrieveRequestStatusAsync(
        string nationalId,
        Guid transactionId,
        string random,
        CancellationToken ctk = default)
    {
        var response = await SendRequestCoreAsync<NafathRetrieveRequestStatusResponse>(
            HttpMethod.Post,
            RetrieveRequestStatusEndpoint,
            new NafathRetrieveRequestStatusRequest(nationalId, transactionId, random),
            ctk);

        return response.RequestStatus;
    }

    /// <summary>
    /// Validates the authentication token contained in the specified Nafath callback response asynchronously and
    /// returns the associated claims principal.
    /// </summary>
    /// <param name="callbackResponse">The Nafath callback response containing the authentication token to validate.</param>
    /// <param name="ctk">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="ClaimsPrincipal"/>
    /// representing the authenticated user if the token is valid; otherwise, an exception is thrown.</returns>
    public async Task<ClaimsPrincipal> ValidateTokenAsync(
        [NotNull] NafathCallbackResponse callbackResponse,
        CancellationToken ctk = default)
    {
        var token = callbackResponse.Token;
        var jwtSecurityToken = _jwtSecurityTokenHandler.ReadJwtToken(token);
        var jwk = await RetrieveJwkAsync(jwtSecurityToken.Header["kid"].ToString()!, ctk);

        var parameters = new TokenValidationParameters
        {
            ValidIssuer = _config.Issuer,
            ValidateIssuer = true,

            ValidAudience = _config.Audience,
            ValidateAudience = true,

            IssuerSigningKey = jwk,
            ValidateIssuerSigningKey = true,

            ValidateLifetime = true,
        };

        return _jwtSecurityTokenHandler.ValidateToken(token, parameters, out _);
    }

    private async Task<JsonWebKey> RetrieveJwkAsync(
        string keyId,
        CancellationToken ctk = default)
    {
        if (_cache.TryGetValue<JsonWebKey>(keyId, out var cached))
        {
            return cached!;
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, RetrieveJwkEndpoint);
        request.Headers.Add("kid", keyId);

        var response = await _httpClient.SendAsync(request, ctk);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(ctk);
        return _cache.Set(keyId, new JsonWebKey(json));
    }

    private async Task<TResponse> SendRequestCoreAsync<TResponse>(
        HttpMethod method,
        string url,
        object? body,
        CancellationToken ctk = default)
    {
        using var request = new HttpRequestMessage(method, url)
        {
            Content = body is not null
                ? JsonContent.Create(body, body.GetType(), options: NafathJsonSourceGenContext.Default.Options)
                : null,
        };

        var response = await _httpClient.SendAsync(request, ctk);
        if (response.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.NotFound)
        {
            var message = await response.Content.ReadAsStringAsync(ctk);
            throw new NafathException(message);
        }
        else if (response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.UnprocessableEntity)
        {
            var errorResponse = await response.Content.ReadFromJsonAsync(NafathJsonSourceGenContext.Default.NafathErrorResponse, ctk);
            throw new NafathException("Nafath API call failed.", errorResponse!);
        }

        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<TResponse>(NafathJsonSourceGenContext.Default.Options, ctk))!;
    }

    private static string Format(string format, params object[] args)
        => string.Format(CultureInfo.InvariantCulture, format, args);
}
