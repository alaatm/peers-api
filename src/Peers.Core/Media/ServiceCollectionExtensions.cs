namespace Peers.Core.Media;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds required services for thumbnails generation.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns></returns>
    public static IServiceCollection AddThumbnailGenerator(this IServiceCollection services)
        => services.AddSingleton<IThumbnailGenerator, ThumbnailGenerator>();
}
