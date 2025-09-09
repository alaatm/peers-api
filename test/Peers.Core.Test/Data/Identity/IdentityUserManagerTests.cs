using System.Security.Claims;
using Peers.Core.Data;
using Peers.Core.Data.Identity;
using Peers.Core.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Peers.Core.Test.Data.Identity;

public class IdentityUserManagerTests
{
    [Fact]
    public async Task CreateUserAsync_throws_for_invalid_password()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddEfIdentity<MyContext, TestUser>()
            .AddDbContext<MyContext>(options => options.UseInMemoryDatabase(Guid.NewGuid().ToString()))
            .BuildServiceProvider();

        var user = new TestUser() { Id = 1, UserName = "testuser" };
        using var userManager = services.GetRequiredService<IdentityUserManager<TestUser, MyContext>>();

        // Act & assert
        await Assert.ThrowsAsync<IdentityResultException>(async () => await userManager.CreateUserAsync(true, user, "", [], []));
    }

    [Fact]
    public async Task CreateUserAsync_creates_user_and_adds_userId_and_username_claims()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddEfIdentity<MyContext, TestUser>()
            .AddDbContext<MyContext>(options => options.UseInMemoryDatabase(Guid.NewGuid().ToString()))
            .BuildServiceProvider();

        var user = new TestUser() { Id = 1, UserName = "testuser" };
        var roles = new[] { "Admin", "User" };

        using var roleManager = services.GetRequiredService<IdentityRoleManager<TestUser, MyContext>>();
        await roleManager.CreateRolesAsync(true, roles);

        using var userManager = services.GetRequiredService<IdentityUserManager<TestUser, MyContext>>();

        // Act
        var returnedClaims = await userManager.CreateUserAsync(true, user, "P@ssw0rd", roles, [new Claim("TestClaim", "TestClaimValue")]);

        // Assert
        using var context = services.GetRequiredService<MyContext>();
        var createdUser = await context.Users.FindAsync(user.Id);
        Assert.NotNull(createdUser);
        Assert.Equal(user.UserName, createdUser.UserName);
        Assert.True(await userManager.CheckPasswordAsync(createdUser, "P@ssw0rd"));
        Assert.Equal(roles, await userManager.GetRolesAsync(createdUser));

        var addedClaims = await userManager.GetClaimsAsync(createdUser);
        Assert.Equal(3, addedClaims.Count);
        Assert.Contains(addedClaims, c => c.Type == CustomClaimTypes.Id && c.Value == "1");
        Assert.Contains(addedClaims, c => c.Type == CustomClaimTypes.Username && c.Value == user.UserName);
        Assert.Contains(addedClaims, c => c.Type == "TestClaim" && c.Value == "TestClaimValue");

        Assert.Equal(3, returnedClaims.Length);
        Assert.Contains(returnedClaims, c => c.Type == CustomClaimTypes.Id && c.Value == "1");
        Assert.Contains(returnedClaims, c => c.Type == CustomClaimTypes.Username && c.Value == user.UserName);
        Assert.Contains(returnedClaims, c => c.Type == "TestClaim" && c.Value == "TestClaimValue");
    }

    [Fact]
    public async Task CreateUserAsync_creates_user_with_no_roles_or_additional_claims()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddEfIdentity<MyContext, TestUser>()
            .AddDbContext<MyContext>(options => options.UseInMemoryDatabase(Guid.NewGuid().ToString()))
            .BuildServiceProvider();

        var user = new TestUser() { Id = 1, UserName = "testuser" };

        using var userManager = services.GetRequiredService<IdentityUserManager<TestUser, MyContext>>();

        // Act
        var returnedClaims = await userManager.CreateUserAsync(true, user, "P@ssw0rd", [], []);

        // Assert
        using var context = services.GetRequiredService<MyContext>();
        var createdUser = await context.Users.FindAsync(user.Id);
        Assert.NotNull(createdUser);
        Assert.Equal(user.UserName, createdUser.UserName);
        Assert.True(await userManager.CheckPasswordAsync(createdUser, "P@ssw0rd"));
        Assert.Empty(await userManager.GetRolesAsync(createdUser));

        var addedClaims = await userManager.GetClaimsAsync(createdUser);
        Assert.Equal(2, addedClaims.Count);
        Assert.Contains(addedClaims, c => c.Type == CustomClaimTypes.Id && c.Value == "1");
        Assert.Contains(addedClaims, c => c.Type == CustomClaimTypes.Username && c.Value == user.UserName);

        Assert.Equal(2, returnedClaims.Length);
        Assert.Contains(returnedClaims, c => c.Type == CustomClaimTypes.Id && c.Value == "1");
        Assert.Contains(returnedClaims, c => c.Type == CustomClaimTypes.Username && c.Value == user.UserName);
    }

    [Fact]
    public async Task CreateUserAsync_throws_when_setting_roles_that_dont_exist()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddEfIdentity<MyContext, TestUser>()
            .AddDbContext<MyContext>(options => options.UseInMemoryDatabase(Guid.NewGuid().ToString()))
            .BuildServiceProvider();

        var user = new TestUser() { Id = 1, UserName = "testuser" };

        using var userManager = services.GetRequiredService<IdentityUserManager<TestUser, MyContext>>();

        // Act & assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () => await userManager.CreateUserAsync(true, user, "P@ssw0rd", ["doesNotExist"], []));
        Assert.Equal("One or more roles were not found.", ex.Message);
    }

    [Fact]
    public async Task CreateUserAsync_does_not_save_changes_when_flag_is_not_set()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddEfIdentity<MyContext, TestUser>()
            .AddDbContext<MyContext>(options => options.UseInMemoryDatabase(Guid.NewGuid().ToString()))
            .BuildServiceProvider();

        var user = new TestUser() { Id = 1, UserName = "testuser" };
        var roles = new[] { "Admin", "User" };

        using var roleManager = services.GetRequiredService<IdentityRoleManager<TestUser, MyContext>>();
        await roleManager.CreateRolesAsync(true, roles);

        using var userManager = services.GetRequiredService<IdentityUserManager<TestUser, MyContext>>();

        // Act
        var returnedClaims = await userManager.CreateUserAsync(false, user, "P@ssw0rd", roles, [new Claim("TestClaim", "TestClaimValue")]);

        // Assert
        using var context = services.GetRequiredService<MyContext>();
        var entry = context.Entry(user);
        Assert.Equal(EntityState.Added, entry.State);
    }

    [Fact]
    public async Task GetRolesAndClaimsAsync_returns_user_roles_and_claims()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddEfIdentity<MyContext, TestUser>()
            .AddDbContext<MyContext>(options => options.UseInMemoryDatabase(Guid.NewGuid().ToString()))
            .BuildServiceProvider();

        var user = new TestUser() { Id = 1, UserName = "testuser" };

        using var roleManager = services.GetRequiredService<IdentityRoleManager<TestUser, MyContext>>();
        await roleManager.CreateRolesAsync(true, ["Admin", "User"]);

        using var userManager = services.GetRequiredService<IdentityUserManager<TestUser, MyContext>>();
        await userManager.CreateUserAsync(true, user, "P@ssw0rd", ["Admin", "User"], [new Claim("TestClaim", "TestClaimValue")]);

        // Act
        var (roles, claims) = await userManager.GetRolesAndClaimsAsync(user);

        // Assert
        Assert.Equal(2, roles.Length);
        Assert.Contains(roles, r => r == "Admin");
        Assert.Contains(roles, r => r == "User");

        Assert.Equal(3, claims.Length);
        Assert.Contains(claims, c => c.Type == CustomClaimTypes.Id && c.Value == "1");
        Assert.Contains(claims, c => c.Type == CustomClaimTypes.Username && c.Value == user.UserName);
        Assert.Contains(claims, c => c.Type == "TestClaim" && c.Value == "TestClaimValue");
    }

    [Fact]
    public async Task Throws_NotSupportedException_for_not_supported_methods()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddEfIdentity<MyContext, TestUser>()
            .AddDbContext<MyContext>(options => options.UseInMemoryDatabase(Guid.NewGuid().ToString()))
            .BuildServiceProvider();

        using var userManager = services.GetRequiredService<IdentityUserManager<TestUser, MyContext>>();

        // Act & Assert
        await Assert.ThrowsAsync<NotSupportedException>(() => userManager.CreateAsync(null));
        await Assert.ThrowsAsync<NotSupportedException>(() => userManager.CreateAsync(null, null));
        await Assert.ThrowsAsync<NotSupportedException>(() => userManager.AddToRoleAsync(null, null));
        await Assert.ThrowsAsync<NotSupportedException>(() => userManager.AddToRolesAsync(null, null));
        await Assert.ThrowsAsync<NotSupportedException>(() => userManager.AddClaimAsync(null, null));
        await Assert.ThrowsAsync<NotSupportedException>(() => userManager.AddClaimsAsync(null, null));
        await Assert.ThrowsAsync<NotSupportedException>(() => userManager.AddPasswordAsync(null, null));
    }

    private class MyContext : TestContext
    {
        public MyContext(DbContextOptions<MyContext> options) : base(options) { }
    }
}
