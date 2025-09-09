using System.Reflection;
using Humanizer;
using Peers.Core.Data;
using Peers.Modules.Customers.Domain;
using Peers.Modules.I18n.Domain;
using Peers.Modules.Media.Domain;
using Peers.Modules.Settings.Domain;
using Peers.Modules.System.Domain;
using Peers.Modules.Users.Domain;

namespace Peers.Modules.Kernel;

public sealed class PeersContext : DbContextBase<AppUser>
{
    public DbSet<Customer> Customers { get; set; } = default!;

    public DbSet<MediaFile> MediaFiles { get; set; } = default!;
    public DbSet<DeviceError> DevicesErrors { get; set; } = default!;
    public DbSet<PushNotificationProblem> PushNotificationProblems { get; set; } = default!;

    public DbSet<Terms> Terms { get; set; } = default!;
    public DbSet<PrivacyPolicy> PrivacyPolicy { get; set; } = default!;
    public DbSet<Language> Languages { get; set; } = default!;
    public DbSet<ClientAppInfo> ClientApps { get; set; } = default!;

    public PeersContext(DbContextOptions options) : base(options)
    {
    }

    private static readonly Type[] _hiloSeqTypes =
    [
        typeof(AppUser),
    ];

    protected override void OnModelCreating([NotNull] ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        foreach (var type in _hiloSeqTypes)
        {
            var seqName = $"{type.Name.Underscore()}_seq";
            builder.HasSequence<int>(seqName).IncrementsBy(100);
            builder.Entity(type).Property<int>("Id").UseHiLo(seqName);
        }

        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
