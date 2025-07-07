namespace Mashkoor.Core.Security.Totp;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds all required services for TOTP token generation.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns></returns>
    public static IServiceCollection AddTotpTokenProvider(this IServiceCollection services)
        => services
            .AddSingleton<ITotpTokenProvider, TotpTokenProvider>();
}

