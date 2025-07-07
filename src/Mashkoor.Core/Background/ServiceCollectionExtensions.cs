using System.Threading.Channels;
using Mashkoor.Core.Domain;

namespace Mashkoor.Core.Background;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds message broker related services to the DI container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="consumerCount">The number of consumers to create.</param>
    /// <returns></returns>
    public static IServiceCollection AddMessageBroker(
        this IServiceCollection services,
        int consumerCount = 1024)
    {
        services.AddSingleton(Channel.CreateUnbounded<ITraceableNotification>());

        services.AddSingleton<IProducer>(ctx =>
        {
            var channel = ctx.GetRequiredService<Channel<ITraceableNotification>>();
            var logger = ctx.GetRequiredService<ILogger<Producer>>();

            return new Producer(channel.Writer, logger);
        });

        services.AddSingleton<IEnumerable<IConsumer>>(ctx =>
        {
            var channel = ctx.GetRequiredService<Channel<ITraceableNotification>>();
            var logger = ctx.GetRequiredService<ILogger<Consumer>>();
            var services = ctx.GetRequiredService<IServiceProvider>();

            var consumers = new Consumer[consumerCount];
            for (var i = 0; i < consumerCount; i++)
            {
                consumers[i] = new Consumer(channel.Reader, services, logger, i + 1);
            }

            return consumers;
        });

        services.AddHostedService<MessageBroker>();
        return services;
    }
}
