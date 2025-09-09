using System.Reflection;
using Peers.Core.AzureServices.Storage;
using Peers.Core.Data.Identity;
using Peers.Core.Domain.Rules;
using Peers.Modules.I18n.Domain;
using Peers.Modules.Settings.Domain;
using Peers.Modules.System.Domain;
using Peers.Modules.Users.Domain;

namespace Peers.Modules.Kernel.Startup;

public sealed class StartupBackgroundService : BackgroundService
{
    public const string AdminUsername = "alaa@peers.net";

    private static readonly string _assetsPath = Path.Join(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "_assets");

    private static readonly string _termsEn = File.ReadAllText(Path.Join(_assetsPath, "legal/terms.en.txt"));
    private static readonly string _termsAr = File.ReadAllText(Path.Join(_assetsPath, "legal/terms.ar.txt"));
    private static readonly string _termsRu = File.ReadAllText(Path.Join(_assetsPath, "legal/terms.ru.txt"));
    private static readonly string _policyEn = File.ReadAllText(Path.Join(_assetsPath, "legal/privacy.en.txt"));
    private static readonly string _policyAr = File.ReadAllText(Path.Join(_assetsPath, "legal/privacy.ar.txt"));
    private static readonly string _policyRu = File.ReadAllText(Path.Join(_assetsPath, "legal/privacy.ru.txt"));

    private readonly TimeProvider _timeProvider;
    private readonly IStorageManager _storageManager;
    private readonly IServiceProvider _serviceProvider;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<StartupBackgroundService> _log;

    public StartupBackgroundService(
        TimeProvider timeProvider,
        IStorageManager storageManager,
        IServiceProvider serviceProvider,
        IWebHostEnvironment env,
        IStringLocalizerFactory stringLocalizerFactory,
        ILogger<StartupBackgroundService> log)
    {
        _timeProvider = timeProvider;
        _storageManager = storageManager;
        _serviceProvider = serviceProvider;
        _env = env;
        _log = log;
        BusinessRule.StringLocalizerFactory = stringLocalizerFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var _ = new TaskTimer(_log, null);
        using var scope = _serviceProvider.CreateScope();

        try
        {
            var context = scope.ServiceProvider.GetRequiredService<PeersContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<IdentityUserManager<AppUser, PeersContext>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<IdentityRoleManager<AppUser, PeersContext>>();

            await CreateStorageAsync();
            await SeedDefaultsAsync(context, userManager, roleManager);
        }
        catch (Exception ex)
        {
            _log.StartupTasksFailed(ex);
            throw;
        }
    }

    private async Task SeedDefaultsAsync(
        PeersContext context,
        IdentityUserManager<AppUser, PeersContext> userManager,
        IdentityRoleManager<AppUser, PeersContext> roleManager)
    {
        if (_env.IsDevelopment())
        {
            await context.Database.MigrateAsync();
        }

        if (!await context.Roles.AnyAsync())
        {
            await AddDefaultRolesAsync(roleManager);
        }

        if (!await context.Users.AnyAsync())
        {
            await SeedAdminUsersAsync(userManager);
        }

        if (!await context.Languages.AnyAsync())
        {
            await SeedDefaultDataAsync(context);
        }
    }

    private async Task AddDefaultRolesAsync(IdentityRoleManager<AppUser, PeersContext> roleManager)
    {
        using var _ = new TaskTimer(_log, "Default roles seed");
        await roleManager.CreateRolesAsync(true, Roles.Default);
    }

    private async Task SeedAdminUsersAsync(IdentityUserManager<AppUser, PeersContext> userManager)
    {
        using var _ = new TaskTimer(_log, "Admin users seed");

        string[] allRoles =
        [
            Roles.Admin, Roles.UsersManager, Roles.SettingsManager, Roles.FinanceAdmin, Roles.Staff,
        ];
        string[] superRoles = [.. allRoles, Roles.MaintenanceAdmin, Roles.PowerAdmin];

        var now = _timeProvider.UtcNow();
        await AddAdmin(now, userManager, AdminUsername, "506494560", "Alaa", "Masoud", superRoles);

        static async Task AddAdmin(
            DateTime date,
            IdentityUserManager<AppUser, PeersContext> um,
            string username,
            string? phoneNumber,
            string firstname,
            string lastname,
            params string[] roles)
        {
            var user = AppUser.CreateStaffAccount(date, username, phoneNumber, firstname, lastname);
            await um.CreateUserAsync(true, user, "P@ssword", roles, []);
        }
    }

    private async Task SeedDefaultDataAsync(PeersContext context)
    {
        using var _ = new TaskTimer(_log, "Default data seed");

        var en = Language.En;
        var ar = Language.Ar;
        var ru = Language.Ru;

        var strategy = context.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await context.Database.BeginTransactionAsync();

            await context.Languages.AddRangeAsync(en, ar, ru);

            await context.Terms.AddAsync(
                Terms.Create(
                    TranslatedField.CreateList((en, "Terms of Service"), (ar, "شروط الخدمة"), (ru, "Условия обслуживания")),
                    TranslatedField.CreateList((en, _termsEn), (ar, _termsAr), (ru, _termsRu)))
            );

            await context.PrivacyPolicy.AddAsync(
                PrivacyPolicy.Create(
                    TranslatedField.CreateList((en, "Privacy Policy"), (ar, "سياسة الخصوصية"), (ru, "Политика конфиденциальности")),
                    TranslatedField.CreateList((en, _policyEn), (ar, _policyAr), (ru, _policyRu)),
                    new(2025, 7, 9))
            );

            await context.ClientApps.AddAsync(new ClientAppInfo
            {
                PackageName = "com.peersapp.peers",
                AndroidStoreLink = "a",
                IOSStoreLink = "b",
                HashString = "h",
                LatestVersion = new ClientAppVersion { Major = 0, Minor = 0, Build = 0, Revision = 0 },
            });

            await context.SaveChangesAsync();
            await transaction.CommitAsync();
        });
    }

    private async Task CreateStorageAsync()
    {
        using var _ = new TaskTimer(_log, "Storage containers creation");
        await _storageManager.CreateContainerAsync(Media.Domain.MediaFile.ContainerName);
    }
}
