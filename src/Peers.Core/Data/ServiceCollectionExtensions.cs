using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Peers.Core.Data.Identity;

namespace Peers.Core.Data;

public static partial class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all required data services in the DI.
    /// </summary>
    /// <typeparam name="TContext">The database context type.</typeparam>
    /// <typeparam name="TContextFactory">The database context factory type.</typeparam>
    /// <typeparam name="TUser">The database user type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="cfg">The database configuration.</param>
    /// <returns></returns>
    public static IServiceCollection AddDataServices<TContext, TContextFactory, TUser>(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> cfg)
        where TContext : DbContextBase<TUser>
        where TContextFactory : class, IDbContextFactory<TContext>
        where TUser : IdentityUserBase
    {
        ArgumentNullException.ThrowIfNull(cfg);

        services
            .Configure<DataProtectionTokenProviderOptions>(p => p.TokenLifespan = TimeSpan.FromMinutes(5))
            .AddPooledDbContextFactory<TContext>(o => cfg(o))
            .AddScoped(sp => sp.GetRequiredService<TContextFactory>().CreateDbContext())
            .AddEfIdentity<TContext, TUser>();

        // Do not register the factory if it is the default one as it is already registered by AddPooledDbContextFactory.
        if (typeof(TContextFactory) != typeof(IDbContextFactory<TContext>))
        {
            services.AddScoped<TContextFactory>();
        }

        return services;
    }

    public static IServiceCollection AddEfIdentity<TContext, TUser>(this IServiceCollection services)
        where TContext : DbContextBase<TUser>
        where TUser : IdentityUserBase
    {
        services
            .AddIdentityCore<TUser>(cfg =>
            {
                // Note: Changing below password requirements requires also changing password validator in
                // enrollment, change password and reset password commands.

                // Keep it simple for now.
                cfg.Password.RequiredLength = 6;
                cfg.Password.RequireUppercase =
                cfg.Password.RequireLowercase =
                cfg.Password.RequireDigit =
                cfg.Password.RequireNonAlphanumeric = false;

                cfg.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultEmailProvider;
                cfg.Tokens.ChangeEmailTokenProvider = TokenOptions.DefaultEmailProvider;
            })
            .AddRoles<IdentityRole<int>>()
            .AddTokenProvider<EmailTokenProvider<TUser>>(TokenOptions.DefaultEmailProvider);

        services.TryAddScoped<IUserStore<TUser>, UserStore<TUser, IdentityRole<int>, TContext, int>>();
        services.TryAddScoped<IRoleStore<IdentityRole<int>>, RoleStore<IdentityRole<int>, TContext, int>>();
        services.AddScoped<IdentityUserManager<TUser, TContext>>();
        services.AddScoped<IdentityRoleManager<TUser, TContext>>();

        return services;
    }
}
