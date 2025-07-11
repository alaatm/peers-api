using System.Reflection;
using Mashkoor.Core.AzureServices;
using Mashkoor.Core.Background;
using Mashkoor.Core.Communication.Email;
using Mashkoor.Core.Communication.Push;
using Mashkoor.Core.Communication.Sms;
using Mashkoor.Core.Data;
using Mashkoor.Core.Localization;
using Mashkoor.Core.RateLimiting;
using Mashkoor.Core.Security.Hashing;
using Mashkoor.Core.Security.Jwt;
using Mashkoor.Core.Security.Totp;
using Mashkoor.Modules.BackgroundJobs;
using Mashkoor.Modules.Kernel.Pipelines;
using Mashkoor.Modules.Kernel.Startup;
using Mashkoor.Modules.Users.Domain;
using Mashkoor.Modules.Users.Services;

namespace Mashkoor.Modules.Kernel;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds all required services for the system to operate.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="config">The builder configuration.</param>
    /// <param name="env">The hosting environment.</param>
    /// <returns></returns>
    public static IServiceCollection AddMashkoor(
        this IServiceCollection services,
        [NotNull] IConfiguration config,
        [NotNull] IWebHostEnvironment env)
    {
        services.AddHostedService<StartupBackgroundService>();
        services.AddMvcCore().AddRazorPages();

        AddMashkoor(
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
                    .TranslateParameterizedCollectionsToConstants());

            });

        if (env.IsDevelopment())
        {
            services.AddOpenApi();
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
    internal static IServiceCollection AddMashkoor(
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

        return services
            .AddSingleton(TimeProvider.System)
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
            .AddTotpTokenProvider()
            .AddCqrs(cfg => cfg.AddOpenBehavior(typeof(IdentityCheckBehavior<,>)), mergedAssemblies)
            .AddPushNotificationProblemReporter()
            .AddDataServices<MashkoorContext, MashkoorContextScopedFactory, AppUser>(cfg)
            .AddMessageBroker()
            .AddBackgroundJobs();
    }
}
