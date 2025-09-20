using System.Globalization;
using System.Reflection;
using Humanizer;
using Microsoft.EntityFrameworkCore.Storage;
using Peers.Core.Data;
using Peers.Modules.Catalog.Domain;
using Peers.Modules.Customers.Domain;
using Peers.Modules.I18n.Domain;
using Peers.Modules.Lookup.Domain;
using Peers.Modules.Media.Domain;
using Peers.Modules.Settings.Domain;
using Peers.Modules.System.Domain;
using Peers.Modules.Users.Domain;

namespace Peers.Modules.Kernel;

public sealed class PeersContext : DbContextBase<AppUser>
{
    public DbSet<ProductType> ProductTypes => Set<ProductType>();
    public DbSet<LookupType> LookupTypes => Set<LookupType>();

    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<MediaFile> MediaFiles => Set<MediaFile>();
    public DbSet<DeviceError> DevicesErrors => Set<DeviceError>();
    public DbSet<PushNotificationProblem> PushNotificationProblems => Set<PushNotificationProblem>();

    public DbSet<Terms> Terms => Set<Terms>();
    public DbSet<PrivacyPolicy> PrivacyPolicy => Set<PrivacyPolicy>();
    public DbSet<Language> Languages => Set<Language>();
    public DbSet<ClientAppInfo> ClientApps => Set<ClientAppInfo>();

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

    public async Task<int> AcquireAppLockAsync(IDbContextTransaction trx, string resource, int timeoutMs, CancellationToken ct)
    {
        const string Sql = """
            DECLARE @rc int;
            EXEC @rc = sp_getapplock 
                @Resource = @p_resource, 
                @LockMode = 'Exclusive', 
                @LockOwner = 'Transaction', 
                @LockTimeout = @p_timeout;
            SELECT @rc;
            """;

        await using var cmd = Database.GetDbConnection().CreateCommand();
        cmd.CommandText = Sql;
        cmd.Transaction = trx.GetDbTransaction();

        var p1 = cmd.CreateParameter();
        p1.ParameterName = "@p_resource";
        p1.Value = resource;
        var p2 = cmd.CreateParameter();
        p2.ParameterName = "@p_timeout";
        p2.Value = timeoutMs;

        cmd.Parameters.Add(p1);
        cmd.Parameters.Add(p2);

        var rcObj = await cmd.ExecuteScalarAsync(ct);
        return Convert.ToInt32(rcObj, CultureInfo.InvariantCulture);
    }
}
