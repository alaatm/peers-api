using System.Data.Common;
using System.Runtime.InteropServices;
using System.Text;
using Peers.Core.AzureServices.Storage;
using Peers.Core.Data;
using Peers.Core.Domain.Rules;
using Peers.Modules.Kernel.Startup;
using Peers.Modules.Media.Domain;
using Peers.Modules.Test.SharedClasses;
using Peers.Modules.Users.Domain;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Peers.Modules.Test.Kernel.Startup;

public class StartupBackgroundServiceTests
{
    private static readonly string _connStr = TestConfig.GetConnectionString("startup", "ConnStrStartup");
    private static readonly IConfiguration _configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
        {
            { "Logging:LogLevel:Default", "None" },
            { "Logging:Console:LogLevel:Default", "None" },
        })
    .Build();

    [Fact]
    public async Task Performs_startup_tasks()
    {
        // Arrange
        if (TestConfig.IsCi && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return;
        }

        var services = new ServiceCollection();
        services
            .AddSingleton(_configuration)
            .AddLocalization()
            .AddDbContext<PeersContext>(cfg => cfg.UseSqlServer(_connStr))
            .AddEfIdentity<PeersContext, AppUser>();

        var serviceProvider = services.BuildServiceProvider();

        var hostMoq = new Mock<IWebHostEnvironment>();
        hostMoq.SetupGet(p => p.EnvironmentName).Returns(Environments.Development);

        var storageManagerMoq = new Mock<IStorageManager>(MockBehavior.Strict);
        storageManagerMoq
            .Setup(p => p.CreateContainerAsync(MediaFile.ContainerName))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var startupService = new StartupBackgroundService(
            TimeProvider.System,
            storageManagerMoq.Object,
            serviceProvider,
            hostMoq.Object,
            MockBuilder.GetLocalizerFactoryMoq().Object,
            Mock.Of<ILogger<StartupBackgroundService>>());

        // Act
        DeleteDatabaseDate();
        // Run twice to ensure idempotency
        await startupService.StartAsync(default);
        await startupService.ExecuteTask;
        await startupService.StartAsync(default);
        await startupService.ExecuteTask;

        // Assert
        Assert.NotNull(BusinessRule.StringLocalizerFactory);

        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PeersContext>();
        Assert.Equal(Roles.Default.Length, await context.Roles.CountAsync());
        Assert.Equal(1, await context.Users.CountAsync());
        Assert.Equal(3, await context.Languages.CountAsync());
        Assert.NotNull(await context.Terms.SingleAsync());
        Assert.NotNull(await context.PrivacyPolicy.SingleAsync());
        storageManagerMoq.VerifyAll();
    }

    private static void DeleteDatabaseDate()
    {
        using var conn = new SqlConnection(_connStr);
        using var cmd = conn.CreateCommand();
        var sb = new StringBuilder();
        try
        {
            conn.Open();
        }
        catch (SqlException)
        {
            return; // Database does not exist, nothing to delete
        }

        cmd.CommandText = "SELECT 'DELETE [' + OBJECT_SCHEMA_NAME(object_id) + '].[' + name + '];' from sys.tables WHERE [name] != '__EFMigrationsHistory'";
        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                sb.Append(reader.GetString(0));
            }

            cmd.CommandText = sb.ToString();
        }

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
    }
}
