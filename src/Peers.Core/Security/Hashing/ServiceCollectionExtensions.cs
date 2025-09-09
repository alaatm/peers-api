namespace Peers.Core.Security.Hashing;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds all required services for HMAC hash services
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns></returns>
    public static IServiceCollection AddHmacHash(this IServiceCollection services)
        => services.AddSingleton<IHmacHash, HmacHash>();
}

