using Humanizer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Peers.Core.Data;
using Peers.Core.Data.Identity;

namespace Peers.Core.Test.Data;

public class ServiceCollectionExtensions
{
    [Fact]
    public void AddDataServices_throws_when_passing_null_config()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => new ServiceCollection().AddDataServices<TestContext, TestContextFactory, TestUser>(null));
        Assert.Equal("cfg", ex.ParamName);
    }

    [Fact]
    public void AddDataServices_registers_required_data_services()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        // Act
        var serviceProvider = serviceCollection
            .AddDataServices<TestContext, TestContextFactory, TestUser>(cfg => cfg.UseInMemoryDatabase(nameof(AddDataServices_registers_required_data_services)))
            .BuildServiceProvider();

        // Assert
        var context = serviceProvider.GetRequiredService<TestContext>();
        Assert.True(context.CreatedViaFactory);
        serviceProvider.GetRequiredService<IDbContextFactory<TestContext>>();
        serviceProvider.GetRequiredService<TestContextFactory>();
        serviceProvider.GetRequiredService<UserManager<TestUser>>();
        serviceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
        serviceProvider.GetRequiredService<EmailTokenProvider<TestUser>>();
        serviceProvider.GetRequiredService<IUserStore<TestUser>>();
        serviceProvider.GetRequiredService<IRoleStore<IdentityRole<int>>>();
        serviceProvider.GetRequiredService<IdentityUserManager<TestUser, TestContext>>();
        serviceProvider.GetRequiredService<IdentityRoleManager<TestUser, TestContext>>();
    }

    [Fact]
    public void AddDataServices_does_not_register_factory_as_scoped_when_its_the_default_factory()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        // Act
        var serviceProvider = serviceCollection
            .AddDataServices<TestContext, IDbContextFactory<TestContext>, TestUser>(cfg => cfg.UseInMemoryDatabase(nameof(AddDataServices_registers_required_data_services)))
            .BuildServiceProvider();

        // Assert
        var context = serviceProvider.GetRequiredService<TestContext>();
        Assert.False(context.CreatedViaFactory);
        serviceProvider.GetRequiredService<IDbContextFactory<TestContext>>();

        using var scope1 = serviceProvider.CreateScope();
        using var scope2 = serviceProvider.CreateScope();

        Assert.Same(scope1.ServiceProvider.GetRequiredService<IDbContextFactory<TestContext>>(), scope2.ServiceProvider.GetRequiredService<IDbContextFactory<TestContext>>());
    }

    [Fact]
    public void Applies_snake_casing_for_columns_and_tables_names()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        // Act
        var serviceProvider = serviceCollection
            .AddDataServices<TestContext, TestContextFactory, TestUser>(cfg => cfg.UseSqlite("DataSource=:memory:"))
            .BuildServiceProvider();

        using var context = serviceProvider.GetRequiredService<TestContext>();

        // Assert
        var conn = context.Database.GetDbConnection();
        conn.Open();
        context.Database.EnsureCreated();

        // Assert custom entities
        using var cmd = context.Database.GetDbConnection().CreateCommand();
        cmd.CommandText = $"pragma table_info({nameof(TestUser).Underscore()});";
        using (var reader = cmd.ExecuteReader())
        {
            var cols = new List<string>();
            while (reader.Read())
            {
                cols.Add(reader.GetString(1));
            }

            Assert.Equal("id", cols[0]);
        }

        // Assert identity entites
        var tables = new List<string>();
        cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table'";
        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                tables.Add(reader.GetString(0));
            }
        }

        // Assert our default tables are created with correct casing
        Assert.Contains("role", tables);
        Assert.Contains("role_claim", tables);
        Assert.Contains("user_claim", tables);
        Assert.Contains("user_login", tables);
        Assert.Contains("user_role", tables);
        Assert.Contains("user_token", tables);
        Assert.Contains(nameof(TestUser).Underscore(), tables);
    }
}
