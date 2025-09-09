using System.Reflection;
using FluentValidation;
using Peers.Core.Cqrs.Pipeline;

namespace Peers.Core.Cqrs;

public static partial class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds CQRS pipeline for logging, authorization and command validation.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <param name="assemblies"></param>
    /// <returns></returns>
    public static IServiceCollection AddCqrs(
        this IServiceCollection services,
        Action<MediatRServiceConfiguration> configuration,
        params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(assemblies);

        var mergedAssemblies = new List<Assembly>(assemblies) { Assembly.GetExecutingAssembly() }.ToArray();

        services
            .AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssemblies(mergedAssemblies);
                cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
                cfg.AddOpenBehavior(typeof(AuthorizationBehavior<,>));
                cfg.AddOpenBehavior(typeof(CommandValidationBehavior<,>));
                configuration(cfg);
            })
            .AddValidatorsFromAssemblies(mergedAssemblies);

        return services;
    }
}
