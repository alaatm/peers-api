using Microsoft.IdentityModel.Tokens;
using Peers.Core.Nafath.Configuration;
using Peers.Core.Nafath.Models;
using Peers.Core.Nafath.Utils;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using C = Peers.Core.Nafath.Models.NafathCallbackConstants;

namespace Peers.Core.Nafath;

public static class EndpointRouteBuilderExtensions
{
    public static RouteGroupBuilder MapNafathCallbackEndpoint(
        [NotNull] this RouteGroupBuilder builder,
        [NotNull] IConfiguration config,
        [NotNull] Func<IServiceProvider, int, NafathIdentity?, Task> callback)
    {
        var callbackUri = config
            .GetSection(NafathConfig.ConfigSection)
            .Get<NafathConfig>()!
            .CallbackUri
            .ToString();

        builder
            .MapPost(callbackUri, async (HttpContext context) =>
            {
                using var reader = new StreamReader(context.Request.Body);
                var json = await reader.ReadToEndAsync();

                if (TryReadData(json, out var data))
                {
                    var services = context.RequestServices;
                    var nafathService = services.GetRequiredService<INafathService>();

                    try
                    {
                        var userId = GuidIntCodec.Decode(data.RequestId);
                        var principal = await nafathService.ValidateTokenAsync(data);
                        var completed = principal.HasClaim(c =>
                            c.Type == C.Status.Namespace &&
                            c.Value == C.Status.Completed);

                        NafathIdentity? identity = null;

                        if (completed)
                        {
                            var nationalId = (principal.FindFirst(C.Attrs.NationalId) ?? principal.FindFirst(C.Attrs.IQamaNumber))?.Value!;
                            var firstNameAr = principal.FindFirst(C.Attrs.FirstNameAr)?.Value;
                            var lastNameAr = (principal.FindFirst(C.Attrs.LastNameAr) ?? principal.FindFirst(C.Attrs.LastNameArNonSaudi))?.Value;
                            var firstNameEn = principal.FindFirst(C.Attrs.FirstNameEn)?.Value;
                            var lastNameEn = principal.FindFirst(C.Attrs.LastNameEn)?.Value;
                            var gender = principal.FindFirst(C.Attrs.Gender)?.Value;

                            identity = new NafathIdentity(nationalId, firstNameAr, lastNameAr, firstNameEn, lastNameEn, gender);
                        }

                        _ = callback(services, userId, identity);
                        return Results.Ok();
                    }
                    catch (SecurityTokenValidationException)
                    {
                        return Results.BadRequest();
                    }
                }

                return Results.BadRequest();
            })
            .AllowAnonymous()
            .ExcludeFromDescription();

        return builder;
    }

    private static bool TryReadData(
        string json,
        [NotNullWhen(true)] out NafathCallbackResponse? data)
    {
        try
        {
            data = JsonSerializer.Deserialize(json, NafathJsonSourceGenContext.Default.NafathCallbackResponse);
            if (data is not null &&
                !string.IsNullOrWhiteSpace(data.Token) &&
                data.TransactionId != Guid.Empty &&
                data.RequestId != Guid.Empty)
            {
                return true;
            }
        }
        catch (JsonException)
        {
        }

        data = null;
        return false;
    }
}
