using System.Reflection;
using Humanizer;
using Mashkoor.Core.Data;
using Mashkoor.Modules.Customers.Domain;
using Mashkoor.Modules.I18n.Domain;
using Mashkoor.Modules.Media.Domain;
using Mashkoor.Modules.Settings.Domain;
using Mashkoor.Modules.System.Domain;
using Mashkoor.Modules.Users.Domain;

namespace Mashkoor.Modules.Kernel;

public sealed class MashkoorContext : DbContextBase<AppUser>
{
    public DbSet<Customer> Customers { get; set; } = default!;

    public DbSet<MediaFile> MediaFiles { get; set; } = default!;
    public DbSet<DeviceError> DevicesErrors { get; set; } = default!;
    public DbSet<PushNotificationProblem> PushNotificationProblems { get; set; } = default!;

    public DbSet<Terms> Terms { get; set; } = default!;
    public DbSet<PrivacyPolicy> PrivacyPolicy { get; set; } = default!;
    public DbSet<Language> Languages { get; set; } = default!;
    public DbSet<ClientAppInfo> ClientApps { get; set; } = default!;

    public MashkoorContext(DbContextOptions options) : base(options)
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
