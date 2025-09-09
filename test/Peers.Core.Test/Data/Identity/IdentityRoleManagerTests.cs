using Peers.Core.Data;
using Peers.Core.Data.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Peers.Core.Test.Data.Identity;

public class IdentityRoleManagerTests
{
    [Fact]
    public async Task CreateRolesAsync_creates_roles()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddEfIdentity<MyContext, TestUser>()
            .AddDbContext<MyContext>(options => options.UseInMemoryDatabase(Guid.NewGuid().ToString()))
            .BuildServiceProvider();

        var roles = new[] { "Admin", "User" };

        using var roleManager = services.GetRequiredService<IdentityRoleManager<TestUser, MyContext>>();

        // Act
        await roleManager.CreateRolesAsync(true, roles);

        // Assert
        using var context = services.GetRequiredService<MyContext>();
        var returnedRoles = await context.Roles.ToListAsync();

        Assert.Equal(2, returnedRoles.Count);
        Assert.Contains(returnedRoles, r => r.Name == "Admin");
        Assert.Contains(returnedRoles, r => r.Name == "User");
    }

    [Fact]
    public async Task CreateRolesAsync_does_not_save_changes_when_flag_is_not_set()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddEfIdentity<MyContext, TestUser>()
            .AddDbContext<MyContext>(options => options.UseInMemoryDatabase(Guid.NewGuid().ToString()))
            .BuildServiceProvider();

        var roles = new[] { "Admin" };

        using var roleManager = services.GetRequiredService<IdentityRoleManager<TestUser, MyContext>>();

        // Act
        await roleManager.CreateRolesAsync(false, roles);

        // Assert
        using var context = services.GetRequiredService<MyContext>();
        Assert.Empty(context.Roles);
    }

    [Fact]
    public async Task Throws_NotSupportedException_for_not_supported_methods()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddEfIdentity<MyContext, TestUser>()
            .AddDbContext<MyContext>(options => options.UseInMemoryDatabase(Guid.NewGuid().ToString()))
            .BuildServiceProvider();

        using var roleManager = services.GetRequiredService<IdentityRoleManager<TestUser, MyContext>>();

        // Act & Assert
        await Assert.ThrowsAsync<NotSupportedException>(() => roleManager.CreateAsync(null));
    }

    private class MyContext : TestContext
    {
        public MyContext(DbContextOptions<MyContext> options) : base(options) { }
    }
}
