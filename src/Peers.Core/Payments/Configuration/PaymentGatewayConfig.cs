using Microsoft.Extensions.Options;
using Peers.Core.Common.Configs;

namespace Peers.Core.Payments.Configuration;

/// <summary>
/// The payment gateway configuration.
/// </summary>
public sealed class PaymentGatewayConfig : IConfigSection
{
    public const string ConfigSection = "PaymentGateway";
    static string IConfigSection.ConfigSection => ConfigSection;

    public PaymentProvider Provider { get; set; }
}

internal sealed class PaymentGatewayConfigValidator : IValidateOptions<PaymentGatewayConfig>
{
    public ValidateOptionsResult Validate(string? name, PaymentGatewayConfig options) => ValidateOptionsResult.Success;
}
