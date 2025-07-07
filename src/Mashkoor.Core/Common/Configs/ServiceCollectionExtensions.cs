using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Options;

namespace Mashkoor.Core.Common.Configs;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers a configuration section and its validator.
    /// </summary>
    /// <typeparam name="TConfig">The configuration section type.</typeparam>
    /// <typeparam name="TConfigValidator">The configuration section validator type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="config">The builder configuration.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection RegisterConfig<TConfig, TConfigValidator>(
        this IServiceCollection services,
        [NotNull] IConfiguration config)
        where TConfig : class, IConfigSection
        where TConfigValidator : class, IValidateOptions<TConfig>
    {
        services.Configure<TConfig>(config.GetSection(TConfig.ConfigSection));
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<TConfig>>().Value);
        services.AddSingleton<IValidateOptions<TConfig>, TConfigValidator>();
        return services;
    }
}
