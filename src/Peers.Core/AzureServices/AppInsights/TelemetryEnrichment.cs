using System.Diagnostics.CodeAnalysis;
using Microsoft.ApplicationInsights.AspNetCore.TelemetryInitializers;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;

namespace Peers.Core.AzureServices.AppInsights;

/// <summary>
/// Ensures to enrich telemetry data with the authenticated user id.
/// </summary>
public sealed class TelemetryEnrichment : TelemetryInitializerBase
{
    public TelemetryEnrichment(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
    {
    }

    protected override void OnInitializeTelemetry(
        [NotNull] HttpContext platformContext,
        [NotNull] RequestTelemetry requestTelemetry,
        [NotNull] ITelemetry telemetry)
        => telemetry.Context.User.AuthenticatedUserId = platformContext.User.Identity?.Name;
}
