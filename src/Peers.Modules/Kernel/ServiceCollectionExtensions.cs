using System.Reflection;
using Peers.Core.AzureServices;
using Peers.Core.Background;
using Peers.Core.Communication.Email;
using Peers.Core.Communication.Push;
using Peers.Core.Communication.Sms;
using Peers.Core.Data;
using Peers.Core.Localization;
using Peers.Core.Media;
using Peers.Core.RateLimiting;
using Peers.Core.Security.Hashing;
using Peers.Core.Security.Jwt;
using Peers.Core.Security.Totp;
using Peers.Modules.BackgroundJobs;
using Peers.Modules.Kernel.OpenApi;
using Peers.Modules.Kernel.Pipelines;
using Peers.Modules.Kernel.Startup;
using Peers.Modules.Users.Domain;
using Peers.Modules.Users.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Peers.Modules.Kernel;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds all required services for the system to operate.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="config">The builder configuration.</param>
    /// <param name="env">The hosting environment.</param>
    /// <returns></returns>
    public static IServiceCollection AddPeers(
        this IServiceCollection services,
        [NotNull] IConfiguration config,
        [NotNull] IWebHostEnvironment env)
    {
        services.AddHostedService<StartupBackgroundService>();
        services.AddMvcCore().AddRazorPages();

        AddPeers(
            services,
            config,
            cfg =>
            {
#if DEBUG
                cfg.EnableThreadSafetyChecks(true);
#else
                cfg.EnableThreadSafetyChecks(false);
#endif
                cfg.UseSqlServer(config.GetConnectionString("Default"), p => p
                    .EnableRetryOnFailure(5)
                    .UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery)
                    .UseParameterizedCollectionMode(ParameterTranslationMode.Constant));

            });

        if (env.IsDevelopment())
        {
            services.AddOpenApiWithTransformers();
        }
        else if (env.IsProduction())
        {
            services.AddAzureAppInsights();
        }

        return services;
    }

    /// <summary>
    /// Adds all required services for the system to operate.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="config">The builder configuration.</param>
    /// <param name="cfg">The database context configuration.</param>
    /// <param name="assemblies">The list of assemblies used for scanning CQRS pipeline</param>
    /// <returns></returns>
    internal static IServiceCollection AddPeers(
        this IServiceCollection services,
        [NotNull] IConfiguration config,
        [NotNull] Action<DbContextOptionsBuilder> cfg,
        params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(cfg);
        ArgumentNullException.ThrowIfNull(assemblies);

        var mergedAssemblies = new List<Assembly>(assemblies) { Assembly.GetExecutingAssembly() }.ToArray();

        //services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(p =>
        //{
        //    p.SerializerOptions.WriteIndented = GlobalJsonOptions.Default.WriteIndented;
        //    p.SerializerOptions.PropertyNameCaseInsensitive = GlobalJsonOptions.Default.PropertyNameCaseInsensitive;
        //    p.SerializerOptions.PropertyNamingPolicy = GlobalJsonOptions.Default.PropertyNamingPolicy;
        //});

        services
            .AddLocalizationWithTracking()
            .AddPushNotifications(config)
            .AddSms(config)
            .AddEmail(config)
            .AddIdentityInfo()
            .AddJwt(config)
            .AddHmacHash()
            //.AddScoped<IPasswordHasher<AppUser>, PasswordHasher<AppUser>>()
            // Technically not used, as AuthorizationBehaviour is used instead but this is still required by other services.
            .AddAuthorization()
            .AddRateLimiting(config)
            .AddTotpTokenProvider(config)
            .AddCqrs(cfg => cfg.AddOpenBehavior(typeof(IdentityCheckBehavior<,>)), mergedAssemblies)
            .AddPushNotificationProblemReporter()
            .AddDataServices<PeersContext, PeersContextScopedFactory, AppUser>(cfg)
            .AddAzureStorage(config)
            .AddMessageBroker()
            .AddBackgroundJobs()
            .AddThumbnailGenerator()
            .TryAddSingleton(TimeProvider.System);

        return services;
    }
}
