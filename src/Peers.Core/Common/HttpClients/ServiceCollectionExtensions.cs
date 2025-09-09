using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Options;
using Peers.Core.Common.Configs;

namespace Peers.Core.Common.HttpClients;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the <see cref="IHttpClientFactory"/> and related services to the <see cref="IServiceCollection"/> and configures
    /// a binding between the <typeparamref name="TClient" /> type and a named <see cref="HttpClient"/>. Also configures the
    /// pooled connection and handler lifetimes. Also adds a standard resilience handler to the <see cref="HttpClient"/>.
    /// </summary>
    /// <typeparam name="TClient">The type of the typed client.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection RegisterHttpClient<TClient>(
        this IServiceCollection services)
        where TClient : class
    {
        const int ConnLifetimeMinutes = 10;
        const int HandlerLifetimeMinutes = 60;

        services
            .AddHttpClient<TClient>()
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler()
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(ConnLifetimeMinutes),
            })
            .SetHandlerLifetime(TimeSpan.FromMinutes(HandlerLifetimeMinutes))
            .AddStandardResilienceHandler();

        return services;
    }

    /// <summary>
    /// Adds the <see cref="IHttpClientFactory"/> and related services to the <see cref="IServiceCollection"/> and configures
    /// a binding between the <typeparamref name="TClient" /> type and a named <see cref="HttpClient"/>. Also configures the
    /// pooled connection and handler lifetimes. Also adds a standard resilience handler to the <see cref="HttpClient"/>.
    /// </summary>
    /// <typeparam name="TClient">The type of the typed client.</typeparam>
    /// <typeparam name="TImplementation">The implementation type of the typed client</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection RegisterHttpClient<TClient, TImplementation>(
        this IServiceCollection services)
        where TClient : class
        where TImplementation : class, TClient
    {
        const int ConnLifetimeMinutes = 10;
        const int HandlerLifetimeMinutes = 60;

        services
            .AddHttpClient<TClient, TImplementation>()
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler()
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(ConnLifetimeMinutes),
            })
            .SetHandlerLifetime(TimeSpan.FromMinutes(HandlerLifetimeMinutes))
            .AddStandardResilienceHandler();

        return services;
    }

    /// <summary>
    /// Adds the <see cref="IHttpClientFactory"/> and related services to the <see cref="IServiceCollection"/> and configures
    /// a binding between the <typeparamref name="TClient" /> type and a named <see cref="HttpClient"/>. Also configures the
    /// pooled connection and handler lifetimes. Also adds a standard resilience handler to the <see cref="HttpClient"/>. Also
    /// registers the configuration and its validator for the typed client.
    /// </summary>
    /// <typeparam name="TClient">The type of the typed client.</typeparam>
    /// <typeparam name="TImplementation">The implementation type of the typed client</typeparam>
    /// <typeparam name="TConfig">The configuration section type.</typeparam>
    /// <typeparam name="TConfigValidator">The configuration section validator type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="config">The builder configuration.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection RegisterHttpClient<TClient, TImplementation, TConfig, TConfigValidator>(
        this IServiceCollection services,
        [NotNull] IConfiguration config)
        where TClient : class
        where TImplementation : class, TClient
        where TConfig : class, IConfigSection
        where TConfigValidator : class, IValidateOptions<TConfig>
        => services
            .RegisterConfig<TConfig, TConfigValidator>(config)
            .RegisterHttpClient<TClient, TImplementation>();
}
