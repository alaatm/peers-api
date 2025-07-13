#pragma warning disable CA1310 // Specify StringComparison for correctness
using System.Data.Common;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Text;
using FirebaseAdmin.Messaging;
using Mashkoor.Core;
using Mashkoor.Core.Background;
using Mashkoor.Core.Commands;
using Mashkoor.Core.Communication.Email;
using Mashkoor.Core.Communication.Push;
using Mashkoor.Core.Communication.Sms;
using Mashkoor.Core.Data.Identity;
using Mashkoor.Core.Domain;
using Mashkoor.Core.Domain.Rules;
using Mashkoor.Core.Http;
using Mashkoor.Core.Identity;
using Mashkoor.Core.Localization;
using Mashkoor.Core.Security.Hashing;
using Mashkoor.Modules.Customers.Domain;
using Mashkoor.Modules.Kernel.Pipelines;
using Mashkoor.Modules.Users.Commands;
using Mashkoor.Modules.Users.Commands.Responses;
using Mashkoor.Modules.Users.Domain;
using Mashkoor.Modules.Users.Events;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Time.Testing;

namespace Mashkoor.Modules.Test;

[CollectionDefinition(nameof(IntegrationTestBaseCollection), DisableParallelization = true)]
public class IntegrationTestBaseCollection { }

public abstract partial class IntegrationTestBase
{
    protected const string DefaultOtp = "9999";
    protected const int JwtDuration = 10;

    public const string TestFirebaseServiceAccount = /*lang=json,strict*/ @"{
  ""type"": ""service_account"",
  ""project_id"": ""test"",
  ""private_key_id"": ""xxx"",
  ""private_key"": ""-----BEGIN PRIVATE KEY-----\nMIIBVgIBADANBgkqhkiG9w0BAQEFAASCAUAwggE8AgEAAkEAq7BFUpkGp3+LQmlQ\nYx2eqzDV+xeG8kx/sQFV18S5JhzGeIJNA72wSeukEPojtqUyX2J0CciPBh7eqclQ\n2zpAswIDAQABAkAgisq4+zRdrzkwH1ITV1vpytnkO/NiHcnePQiOW0VUybPyHoGM\n/jf75C5xET7ZQpBe5kx5VHsPZj0CBb3b+wSRAiEA2mPWCBytosIU/ODRfq6EiV04\nlt6waE7I2uSPqIC20LcCIQDJQYIHQII+3YaPqyhGgqMexuuuGx+lDKD6/Fu/JwPb\n5QIhAKthiYcYKlL9h8bjDsQhZDUACPasjzdsDEdq8inDyLOFAiEAmCr/tZwA3qeA\nZoBzI10DGPIuoKXBd3nk/eBxPkaxlEECIQCNymjsoI7GldtujVnr1qT+3yedLfHK\nsrDVjIT3LsvTqw==\n-----END PRIVATE KEY-----\n"",
  ""client_email"": ""firebase-adminsdk-xxxxxx@test.iam.gserviceaccount.com"",
  ""client_id"": ""xxx"",
  ""auth_uri"": ""https://accounts.google.com/o/oauth2/auth"",
  ""token_uri"": ""https://oauth2.googleapis.com/token"",
  ""auth_provider_x509_cert_url"": ""https://www.googleapis.com/oauth2/v1/certs"",
  ""client_x509_cert_url"": ""https://www.googleapis.com/robot/v1/metadata/x509/firebase-adminsdk-xxxxx%40test.iam.gserviceaccount.com""
}";

    public static readonly string ConnStr = TestConfig.GetConnectionString("integration", "ConnStrIntegration");
    private static readonly IConfiguration _configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
        {
            { "Logging:LogLevel:Default", "None" },
            { "Logging:Console:LogLevel:Default", "None" },
            { "ConnectionStrings:Default", ConnStr },
            { "jwt:issuer", "https://integration-tests.com/iss" },
            { "jwt:key", Convert.ToBase64String(new byte[32]) },
            { "jwt:durationInMinutes", $"{JwtDuration}" },
            { "firebase:serviceAccountKey", TestFirebaseServiceAccount },
            { "totp:useDefaultOtp", "false" },
            { "totp:duration", "00:03:00" },
            { "sms:sender", "Mashkoor" },
            { "sms:key", "123" },
            { "sms:enabled", "false" },
            { "email:host", "smtp.Mashkoor.com" },
            { "email:username", "username" },
            { "email:password", "password" },
            { "email:senderName", "Mashkoor" },
            { "email:senderEmail", "email@Mashkoor.com" },
            { "email:port", "995" },
            { "email:enableSsl", "true" },
            { "email:enabled", "false" },
            { "rateLimiting:perUserRateLimit:queueLimit", "0" },
            { "rateLimiting:perUserRateLimit:tokenLimit", "300" },
            { "rateLimiting:perUserRateLimit:tokensPerPeriod", "300" },
            { "rateLimiting:perUserRateLimit:autoReplenishment", "true" },
            { "rateLimiting:perUserRateLimit:replenishmentPeriod", "60" },
            { "rateLimiting:anonRateLimit:queueLimit", "0" },
            { "rateLimiting:anonRateLimit:tokenLimit", "200" },
            { "rateLimiting:anonRateLimit:tokensPerPeriod", "200" },
            { "rateLimiting:anonRateLimit:autoReplenishment", "true" },
            { "rateLimiting:anonRateLimit:replenishmentPeriod", "60" },
            { "rateLimiting:anonConcurrencyLimit:queueLimit", "0" },
            { "rateLimiting:anonConcurrencyLimit:permitLimit", "10" },
        })
        .Build();

    private readonly Mock<ISmsService> _smsMoq;
    private readonly Mock<IPushNotificationService> _pushMoq;
    private readonly Mock<IEmailService> _emailMoq;

    protected IServiceProvider Services { get; }
    protected FakeTimeProvider TimeProviderMoq { get; }
    protected Mock<IIdentityInfo> IdentityMoq { get; }
    protected Mock<IHmacHash> HmacHashMoq { get; }
    protected Mock<IProducer> ProducerMoq { get; }
    protected Mock<LinkGenerator> LinkGeneratorMoq { get; }
    protected Mock<IHttpContextAccessor> HttpContextAccessorMoq { get; }
    protected Mock<IFirebaseMessagingService> FirebaseMoq { get; }

    public Action<string, string> OnSms
    {
        set => _smsMoq
            .Setup(p => p.SendAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Callback(value);
    }

    public Action<(IReadOnlyList<Message>, IReadOnlyList<MulticastMessage>)> OnPush
    {
        set => _pushMoq
            .Setup(p => p.DispatchAsync(It.IsAny<(IReadOnlyList<Message>, IReadOnlyList<MulticastMessage>)>()))
            .Callback(value);
    }

    public Action<string, string, string> OnEmail
    {
        set => _emailMoq
            .Setup(p => p.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Callback(value);
    }

    public IntegrationTestBase(bool useFakeTimeProvider = false)
    {
        // Skip all integration tests when run in CI under windows
        if (TestConfig.IsCi && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            throw new PlatformNotSupportedException();
        }

        if (useFakeTimeProvider)
        {
            TimeProviderMoq = new FakeTimeProvider();
        }
        IdentityMoq = new Mock<IIdentityInfo>();
        HmacHashMoq = new Mock<IHmacHash>();
        ProducerMoq = new Mock<IProducer>();
        LinkGeneratorMoq = new Mock<LinkGenerator>(MockBehavior.Strict) { CallBase = true };
        HttpContextAccessorMoq = new Mock<IHttpContextAccessor>(MockBehavior.Loose);
        FirebaseMoq = new Mock<IFirebaseMessagingService>(MockBehavior.Loose);

        _smsMoq = new Mock<ISmsService>();
        _pushMoq = new Mock<IPushNotificationService>();
        _emailMoq = new Mock<IEmailService>();

        IdentityCheckBehaviorOptions.Enabled = false;
        var serviceCollection = new ServiceCollection()
            .AddSingleton(_configuration)
            .AddMashkoor(_configuration, cfg => cfg.UseSqlServer(_configuration.GetConnectionString("Default"))/*.EnableSensitiveDataLogging(true)*/)
            .Replace(new ServiceDescriptor(typeof(IIdentityInfo), IdentityMoq.Object))
            .Replace(new ServiceDescriptor(typeof(IHmacHash), HmacHashMoq.Object))
            .Replace(new ServiceDescriptor(typeof(ISmsService), _smsMoq.Object))
            .Replace(new ServiceDescriptor(typeof(IPushNotificationService), _pushMoq.Object))
            .Replace(new ServiceDescriptor(typeof(IEmailService), _emailMoq.Object))
            .Replace(new ServiceDescriptor(typeof(IProducer), ProducerMoq.Object))
            .Replace(new ServiceDescriptor(typeof(LinkGenerator), LinkGeneratorMoq.Object))
            .Replace(new ServiceDescriptor(typeof(IHttpContextAccessor), HttpContextAccessorMoq.Object))
            .Replace(new ServiceDescriptor(typeof(IFirebaseMessagingService), FirebaseMoq.Object));

        if (useFakeTimeProvider)
        {
            serviceCollection.Replace(new ServiceDescriptor(typeof(TimeProvider), TimeProviderMoq));
        }

        ClearIdentity();
        Services = serviceCollection.BuildServiceProvider();

        BusinessRule.StringLocalizerFactory = Services.GetRequiredService<IStringLocalizerFactory>();
        EnsureDatabase();
    }

    public void ExecuteScope(Action<IServiceProvider> action)
    {
        using var scope = Services.CreateScope();
        action(scope.ServiceProvider);
    }

    public T ExecuteScope<T>(Func<IServiceProvider, T> action)
    {
        using var scope = Services.CreateScope();
        return action(scope.ServiceProvider);
    }

    public async Task ExecuteScopeAsync(Func<IServiceProvider, Task> action)
    {
        using var scope = Services.CreateScope();
        await action(scope.ServiceProvider);
    }

    public async Task<T> ExecuteScopeAsync<T>(Func<IServiceProvider, Task<T>> action)
    {
        using var scope = Services.CreateScope();
        return await action(scope.ServiceProvider);
    }

    public void ExecuteDbContext(Action<MashkoorContext> action)
        => ExecuteScope(sp => action(sp.GetService<MashkoorContext>()));

    public T ExecuteDbContext<T>(Func<MashkoorContext, T> action)
        => ExecuteScope(sp => action(sp.GetService<MashkoorContext>()));

    public Task ExecuteDbContextAsync(Func<MashkoorContext, Task> action)
        => ExecuteScopeAsync(sp => action(sp.GetService<MashkoorContext>()));

    public Task<T> ExecuteDbContextAsync<T>(Func<MashkoorContext, Task<T>> action)
        => ExecuteScopeAsync(sp => action(sp.GetService<MashkoorContext>()));

    public Task ExecuteDbContextAsync(Func<MashkoorContext, IMediator, Task> action)
        => ExecuteScopeAsync(sp => action(sp.GetService<MashkoorContext>(), sp.GetService<IMediator>()));

    public Task<T> ExecuteDbContextAsync<T>(Func<MashkoorContext, IMediator, Task<T>> action)
        => ExecuteScopeAsync(sp => action(sp.GetService<MashkoorContext>(), sp.GetService<IMediator>()));

    private readonly Dictionary<string, AppUser> _managers = [];
    private static readonly string[] _allRoles =
    [
        Roles.Admin,
        Roles.UsersManager,
        Roles.SettingsManager,
        Roles.MaintenanceAdmin,
        Roles.FinanceAdmin,
        Roles.Staff,
        Roles.PowerAdmin,
    ];
    private static readonly string _allRolesString = string.Join('_', _allRoles);
    public async Task<AppUser> InsertManagerAsync(string email = null, string password = null, params string[] roles)
    {
        if (roles.Length == 0)
        {
            roles = _allRoles;
        }
        if (!roles.Contains(Roles.Staff))
        {
            roles = [.. roles, Roles.Staff];
        }

        var rolesString = roles.Length == 0 ? _allRolesString : string.Join('_', roles);

        if (_managers.TryGetValue(rolesString, out var manager))
        {
            return manager;
        }

        return await ExecuteScopeAsync(async sp =>
        {
            email ??= new Bogus.DataSets.Internet().Email();
            var names = new Bogus.DataSets.Name();

            var user = AppUser.CreatePasswordAccount(sp.GetRequiredService<TimeProvider>().UtcNow(), email, names.FirstName(), names.LastName(), Lang.EnLangCode);
            user.PhoneNumber = TestPhoneNumber()[4..];
            var um = sp.GetRequiredService<IdentityUserManager<AppUser, MashkoorContext>>();
            await um.CreateUserAsync(true, user, password ?? "P@ssword", roles, []);

            _managers[rolesString] = user;
            return user;
        });
    }

    public async Task<Customer> EnrollCustomer(
        string username = null,
        bool isBanned = false,
        bool isSuspended = false,
        bool registerDevice = true)
    {
        username ??= TestPhoneNumber();

        ProducerMoq.Setup(p => p.PublishAsync(It.IsAny<EnrollRequested>(), It.IsAny<CancellationToken>())).Callback(() =>
            Services.GetRequiredService<IMemoryCache>().Set(username, DefaultOtp, TimeSpan.FromDays(1)));

        HmacHashMoq
            .Setup(p => p.GenerateKey())
            .Returns(new HmacHash().GenerateKey());

        AssertX.IsType<Accepted<OtpResponse>>(await SendAsync(TestEnroll().Generate() with { Username = username }));
        AssertX.IsType<Ok<JwtResponse>>(await SendAsync(TestEnrollConfirm().Generate() with { Username = username, Otp = DefaultOtp }));

        var customer = await FindAsync<Customer>(p => p.Username == username, "User");
        if (registerDevice)
        {
            AssertX.IsType<Created<IdObj>>(await SendAsync(TestRegisterDevice.Generate(), customer));
        }

        if (isBanned)
        {
            await Ban(customer.Id);
        }

        if (isSuspended)
        {
            await Suspend(customer.Id);
        }

        customer = await FindAsync<Customer>(p => p.Username == username, "User", "User.RefreshTokens", "User.DeviceList");
        return customer;
    }

    private async Task Ban(int id)
    {
        var manager = await InsertManagerAsync();
        await SendAsync(new ChangeStatus.Command(id, UserStatus.Banned, "test"), manager);
    }

    private async Task Suspend(int id)
    {
        var manager = await InsertManagerAsync();
        await SendAsync(new ChangeStatus.Command(id, UserStatus.Suspended, "test"), manager);
    }

    public Task InsertAsync<TEntity>(params TEntity[] entities) where TEntity : class
        => ExecuteDbContextAsync(db =>
        {
            foreach (var entity in entities)
            {
                db.Set<TEntity>().Add(entity);
            }
            return db.SaveChangesAsync();
        });

    public Task InsertAsync<TEntity>(TEntity entity) where TEntity : class
        => ExecuteDbContextAsync(db =>
        {
            db.Set<TEntity>().Add(entity);
            return db.SaveChangesAsync();
        });

    public Task InsertAsync<TEntity1, TEntity2>(TEntity1 entity1, TEntity2 entity2)
        where TEntity1 : class
        where TEntity2 : class
        => ExecuteDbContextAsync(db =>
        {
            db.Set<TEntity1>().Add(entity1);
            db.Set<TEntity2>().Add(entity2);
            return db.SaveChangesAsync();
        });

    public Task InsertAsync<TEntity1, TEntity2, TEntity3>(TEntity1 entity1, TEntity2 entity2, TEntity3 entity3)
        where TEntity1 : class
        where TEntity2 : class
        where TEntity3 : class
        => ExecuteDbContextAsync(db =>
        {
            db.Set<TEntity1>().Add(entity1);
            db.Set<TEntity2>().Add(entity2);
            db.Set<TEntity3>().Add(entity3);
            return db.SaveChangesAsync();
        });

    public Task InsertAsync<TEntity1, TEntity2, TEntity3, TEntity4>(TEntity1 entity1, TEntity2 entity2, TEntity3 entity3, TEntity4 entity4)
        where TEntity1 : class
        where TEntity2 : class
        where TEntity3 : class
        where TEntity4 : class
        => ExecuteDbContextAsync(db =>
        {
            db.Set<TEntity1>().Add(entity1);
            db.Set<TEntity2>().Add(entity2);
            db.Set<TEntity3>().Add(entity3);
            db.Set<TEntity4>().Add(entity4);
            return db.SaveChangesAsync();
        });

    public TEntity Find<TEntity>(int id, params string[] includes) where TEntity : class, IEntity
        => ExecuteDbContext(db =>
        {
            var q = db.Set<TEntity>().AsNoTracking();
            foreach (var include in includes)
            {
                q = q.Include(include);
            }
            return q.FirstOrDefault(p => p.Id == id);
        });

    public TEntity Find<TEntity>(Expression<Func<TEntity, bool>> predicate, params string[] includes) where TEntity : class
        => ExecuteDbContext(db =>
        {
            var q = db.Set<TEntity>().AsNoTracking();
            foreach (var include in includes)
            {
                q = q.Include(include);
            }
            return q.FirstOrDefault(predicate);
        });

    public Task<TEntity[]> FindAllAsync<TEntity>(int[] ids, params string[] includes) where TEntity : class, IEntity
        => ExecuteDbContextAsync(async db =>
        {
            var q = db.Set<TEntity>().AsNoTracking();
            foreach (var include in includes)
            {
                q = q.Include(include);
            }
            var list = await q.Where(p => ids.Contains(p.Id)).ToArrayAsync();
            return ids.Select(id => list.Single(p => p.Id == id)).ToArray();
        });

    public Task<TEntity> FindAsync<TEntity>(int id, params string[] includes) where TEntity : class, IEntity
        => ExecuteDbContextAsync(db =>
        {
            var q = db.Set<TEntity>().AsNoTracking();
            foreach (var include in includes)
            {
                q = q.Include(include);
            }
            return q.FirstOrDefaultAsync(p => p.Id == id);
        });

    public Task<TEntity> FindAsyncTracking<TEntity>(int id, params string[] includes) where TEntity : class, IEntity
        => ExecuteDbContextAsync(db =>
        {
            var q = db.Set<TEntity>().AsNoTrackingWithIdentityResolution();
            foreach (var include in includes)
            {
                q = q.Include(include);
            }
            return q.FirstOrDefaultAsync(p => p.Id == id);
        });

    public Task<TEntity> FindAsync<TEntity>(Expression<Func<TEntity, bool>> predicate, params string[] includes) where TEntity : class
        => ExecuteDbContextAsync(db =>
        {
            var q = db.Set<TEntity>().AsNoTracking();
            foreach (var include in includes)
            {
                q = q.Include(include);
            }
            return q.FirstOrDefaultAsync(predicate);
        });

    public async Task<IResult> SendAsync(ICommand cmd, AppUser executionContext)
    {
        await SetIdentity(executionContext);
        var result = await SendAsync(cmd);
        ClearIdentity();
        return result;
    }

    public Task<IResult> SendAsync(ICommand cmd, ISystemUser executionContext)
        => SendAsync(cmd, executionContext.User);

    public Task<IResult> SendAsync(ICommand cmd)
        => ExecuteScopeAsync(sp =>
        {
            var mediator = sp.GetRequiredService<IMediator>();
            return mediator.Send(cmd);
        });

    private readonly Dictionary<int, string[]> _rolesCache = [];
    private async Task SetIdentity(AppUser user)
    {
        if (_rolesCache.TryGetValue(user.Id, out var roles))
        {
            SetIdentity(user.Id, user.UserName, roles);
        }
        else
        {
            roles = [];

            await ExecuteScopeAsync(async sp =>
            {
                var um = sp.GetRequiredService<UserManager<AppUser>>();
                roles = [.. await um.GetRolesAsync(user)];
            });

            SetIdentity(user.Id, user.UserName, roles);
            _rolesCache[user.Id] = roles;
        }
    }

    private void SetIdentity(int userId, string username, params string[] roles)
    {
        IdentityMoq.SetupGet(p => p.Id).Returns(userId).Verifiable();
        IdentityMoq.SetupGet(p => p.Username).Returns(username).Verifiable();
        IdentityMoq.SetupGet(p => p.IsAuthenticated).Returns(true);
        IdentityMoq.Setup(p => p.IsInRole(It.IsAny<string>())).Returns((string role) => roles.Contains(role));
    }

    protected void ClearIdentity() => IdentityMoq.Reset();

    protected Task AssertAssignedRolesAndClaimsAsync(AppUser user, string[] expectedRoles, Claim[] expectedClaims)
    {
        var um = Services.GetRequiredService<IdentityUserManager<AppUser, MashkoorContext>>();
        return um.AssertAssignedRolesAndClaimsAsync(user, expectedRoles, expectedClaims);
    }

    protected static void AssertAssignedRolesAndClaimsInJwt(string token, AppUser user, string[] expectedRoles, Claim[] expectedClaims)
    {
        var ignoredClaims = new[] { "aud", "exp", "iat", "iss", "jti", CustomClaimTypes.Role };

        var handler = new JwtSecurityTokenHandler();
        var jwtSecurityToken = handler.ReadJwtToken(token);
        var rolesSorted = jwtSecurityToken.Claims.Where(p => p.Type == CustomClaimTypes.Role).Select(p => p.Value).Order().ToArray();
        var claimsSorted = jwtSecurityToken.Claims.Where(p => !ignoredClaims.Contains(p.Type)).OrderBy(p => p.Type).ToArray();

        Assert.Equal(expectedRoles.Order(), rolesSorted);

        var expectedClaimsSorted = expectedClaims.OrderBy(p => p.Type).ToArray();
        Assert.Equal(expectedClaimsSorted.Length, claimsSorted.Length);
        foreach (var expectedClaim in expectedClaimsSorted)
        {
            var actualClaim = claimsSorted.Single(p => p.Type == expectedClaim.Type);
            Assert.Equal(expectedClaim.Type, actualClaim.Type);
            Assert.Equal(expectedClaim.Value, actualClaim.Value);
        }

        var actualSubClaim = claimsSorted.Single(p => p.Type == CustomClaimTypes.Id);
        Assert.Equal(user.Id.ToString(CultureInfo.InvariantCulture), actualSubClaim.Value);
        var actualNameClaim = claimsSorted.Single(p => p.Type == CustomClaimTypes.Username);
        Assert.Equal(user.UserName, actualNameClaim.Value);
    }

    protected async Task AssertCommandAccess(ICommand command, string[] requiredRoles)
    {
        // Authenticated
        foreach (var role in Roles.Default)
        {
            SetIdentity(0, "username", role);

            if (requiredRoles.Contains(role))
            {
                try
                {
                    Assert.IsNotType<UnauthorizedHttpResult2<ProblemDetails>>(await SendAsync(command));
                }
                catch (MockException) { }
                catch (InvalidOperationException) { }
                catch (Exception e) when (e.Message.StartsWith("Method Debug.Fail failed with", StringComparison.OrdinalIgnoreCase)) { }
            }
            else
            {
                AssertX.IsType<ForbiddenHttpResult<ProblemDetails>>(await SendAsync(command));
            }
        }

        // Non-authenticated
        await AssertCommandAccess(command);
    }

    protected async Task AssertCommandAccess(ICommand command)
    {
        // Non-authenticated
        ClearIdentity();
        AssertX.IsType<UnauthorizedHttpResult2<ProblemDetails>>(await SendAsync(command));
    }

    protected async Task AssertCommandAccessNoAuth(ICommand command)
    {
        // Non-authenticated
        ClearIdentity();
        var result = await SendAsync(command);
        Assert.True(result.GetType() != typeof(UnauthorizedHttpResult2<ProblemDetails>), "Command should not require authentication");
    }

    private static string _deleteSql;
    private static bool _dbCreated;
    public static bool DefaultsSeeded { get; set; }
    private void EnsureDatabase()
    {
        if (!_dbCreated)
        {
            using var scope = Services.CreateScope();
            scope.ServiceProvider.GetRequiredService<MashkoorContext>().Database.EnsureCreated();
            _dbCreated = true;
        }

        using var conn = new SqlConnection(ConnStr);
        using var cmd = conn.CreateCommand();
        var sb = new StringBuilder();
        conn.Open();

        if (_deleteSql is null)
        {
            cmd.CommandText = "SELECT 'DELETE [' + OBJECT_SCHEMA_NAME(object_id) + '].[' + name + '];' from sys.tables WHERE temporal_type != 1 and name not in ('role', 'language')";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                sb.Append(reader.GetString(0));
            }
            _deleteSql = sb.ToString();
        }

        cmd.CommandText = _deleteSql;

        const int MaxRetries = 5;
        var retry = 0;
        var hasError = true;
        while (hasError && retry < MaxRetries)
        {
            try
            {
                cmd.ExecuteNonQuery();
                hasError = false;
            }
            catch (DbException)
            {
                retry++;
            }
        }

        if (!DefaultsSeeded)
        {
            cmd.CommandText = "SELECT COUNT(1) FROM [id].[role]";
            var roleCount = (int)cmd.ExecuteScalar();

            if (roleCount == 0)
            {
                sb.Clear();

                for (var i = 0; i < Roles.Default.Length; i++)
                {
                    var role = Roles.Default[i];
                    sb.Append($"(null,N'{role}', N'{role.ToUpperInvariant()}')");
                    sb.Append(i < Roles.Default.Length - 1 ? ',' : ';');
                }

                cmd.CommandText = $"INSERT INTO [id].[role] ([concurrency_stamp], [name], [normalized_name]) VALUES {sb}";
                cmd.ExecuteNonQuery();
            }

            cmd.CommandText = """
                IF NOT EXISTS (SELECT 1 FROM [i18n].[language])
                BEGIN
                    INSERT INTO [i18n].[language] ([id], [name]) VALUES (N'en', N'English'), (N'ar', N'Arabic'), (N'ru', N'Russian');
                END
                """;
            cmd.ExecuteNonQuery();

            DefaultsSeeded = true;
        }

        conn.Close();
    }
}
#pragma warning restore CA1310 // Specify StringComparison for correctness
