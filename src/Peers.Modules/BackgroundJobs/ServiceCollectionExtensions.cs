namespace Peers.Modules.BackgroundJobs;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all required background jobs.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns></returns>
    public static IServiceCollection AddBackgroundJobs(this IServiceCollection services) => services;
    //.AddBackgroundJob<ExpiredBookingsJob>()
    //.AddBackgroundJob<StalledPreAssignmentsJob>()
    //.AddBackgroundJob<ExpiringDocumentsReminderJob>()
    //.AddBackgroundJob<ScheduledBookingsCheckerJob>()
    //.AddBackgroundJob<InactiveOnlineDriversCheckerJob>()
    //.AddBackgroundJob<PayoutCyclesGeneratorJob>();
}
