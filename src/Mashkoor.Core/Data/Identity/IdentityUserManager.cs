using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Mashkoor.Core.Identity;

namespace Mashkoor.Core.Data.Identity;

/// <summary>
/// Provides methods for managing users in a persistence store.
/// </summary>
/// <typeparam name="TUser">The type representing a user.</typeparam>
/// <typeparam name="TContext">The type of the data context class used to access the persistence store.</typeparam>
public sealed class IdentityUserManager<TUser, TContext> : UserManager<TUser>
    where TUser : IdentityUser<int>
    where TContext : IdentityDbContext<TUser, IdentityRole<int>, int>
{
    private readonly TContext _context;

    public IdentityUserManager(
        TContext context,
        IUserStore<TUser> store,
        IOptions<IdentityOptions> optionsAccessor,
        IPasswordHasher<TUser> passwordHasher,
        IEnumerable<IPasswordValidator<TUser>> passwordValidators,
        ILookupNormalizer lookupNormalizer,
        IdentityErrorDescriber errors,
        IServiceProvider services,
        ILogger<UserManager<TUser>> logger) : base(store, optionsAccessor, passwordHasher, [], passwordValidators, lookupNormalizer, errors, services, logger)
        => _context = context;

    /// <summary>
    /// Creates the specified user in the persistence store and appends userId and userName claims to the claims array.
    /// </summary>
    /// <param name="autoSave">Whether to automatically save changes to the persistence store.</param>
    /// <param name="user">The user to create.</param>
    /// <param name="password">The password for the user.</param>
    /// <param name="roles">The roles to assign to the user.</param>
    /// <param name="claims">The claims to assign to the user.</param>
    /// <returns></returns>
    /// <exception cref="IdentityResultException"></exception>
    public async Task<Claim[]> CreateUserAsync(bool autoSave, TUser user, string password, string[] roles, Claim[] claims)
    {
        if (await UpdatePasswordHash(user, password, true).ConfigureAwait(false) is { Succeeded: false } result)
        {
            throw new IdentityResultException(result.Errors.Select(p => p.Description));
        }

        return await CreateUserAsync(autoSave, user, roles, claims).ConfigureAwait(false);
    }

    /// <summary>
    /// Creates the specified user in the persistence store and appends userId and userName claims to the claims array.
    /// </summary>
    /// <param name="autoSave">Whether to automatically save changes to the persistence store.</param>
    /// <param name="user">The user to create.</param>
    /// <param name="roles">The roles to assign to the user.</param>
    /// <param name="claims">The claims to assign to the user.</param>
    /// <returns></returns>
    /// <exception cref="IdentityResultException"></exception>
    public async Task<Claim[]> CreateUserAsync(
        bool autoSave,
        [NotNull] TUser user,
        [NotNull] string[] roles,
        [NotNull] Claim[] claims)
    {
        ((UserStore<TUser, IdentityRole<int>, TContext, int>)Store).AutoSaveChanges = false;

        if (await UpdateSecurityStampAsync(user).ConfigureAwait(false) is { Succeeded: false } result)
        {
            throw new IdentityResultException(result.Errors.Select(p => p.Description));
        }

        await UpdateNormalizedUserNameAsync(user).ConfigureAwait(false);
        await UpdateNormalizedEmailAsync(user).ConfigureAwait(false);

        await _context.Users.AddAsync(user).ConfigureAwait(false);

        var allClaims = new Claim[claims.Length + 2];
        allClaims[0] = new Claim(CustomClaimTypes.Id, user.Id.ToString(CultureInfo.InvariantCulture), ClaimValueTypes.Integer);
        allClaims[1] = new Claim(CustomClaimTypes.Username, user.UserName!);
        Array.Copy(claims, 0, allClaims, 2, claims.Length);

        await AddUserRolesAsync(user, roles).ConfigureAwait(false);
        await AddUserClaimsAsync(user, allClaims).ConfigureAwait(false);
        await SaveChangesAsync(autoSave).ConfigureAwait(false);

        return allClaims;
    }

    /// <summary>
    /// Retrieves the roles and claims for the specified user.
    /// </summary>
    /// <param name="user">The user to retrieve the roles and claims for.</param>
    /// <returns></returns>
    public async Task<(string[] roles, Claim[] claims)> GetRolesAndClaimsAsync(TUser user)
    {
        const string RoleClaimType = "Role";

        var roleClaims =
            from userRole in _context.UserRoles
            join role in _context.Roles on userRole.RoleId equals role.Id
            where userRole.UserId == user.Id
            select new { Type = RoleClaimType, Value = role.Name };

        var userClaims =
            from userClaim in _context.UserClaims
            where userClaim.UserId == user.Id
            select new { Type = userClaim.ClaimType, Value = userClaim.ClaimValue };

        var rolesAndClaims = await roleClaims
            .Concat(userClaims)
            .ToArrayAsync();

        var roles = rolesAndClaims
            .Where(c => c.Type == RoleClaimType)
            .Select(c => c.Value)
            .ToArray();

        var claims = rolesAndClaims
            .Where(c => c.Type != RoleClaimType)
            .Select(p => new Claim(p.Type, p.Value))
            .ToArray();

        return (roles, claims);
    }

    public override Task<IdentityResult> CreateAsync(TUser user) => throw new NotSupportedException();
    public override Task<IdentityResult> CreateAsync(TUser user, string password) => throw new NotSupportedException();
    public override Task<IdentityResult> AddToRoleAsync(TUser user, string role) => throw new NotSupportedException();
    public override Task<IdentityResult> AddToRolesAsync(TUser user, IEnumerable<string> roles) => throw new NotSupportedException();
    public override Task<IdentityResult> AddClaimAsync(TUser user, Claim claim) => throw new NotSupportedException();
    public override Task<IdentityResult> AddClaimsAsync(TUser user, IEnumerable<Claim> claims) => throw new NotSupportedException();
    public override Task<IdentityResult> AddPasswordAsync(TUser user, string password) => throw new NotSupportedException();

    private async Task AddUserRolesAsync(TUser user, string[] roles)
    {
        if (roles.Length == 0)
        {
            return;
        }

        var normalizedRoles = new string[roles.Length];
        for (var i = 0; i < roles.Length; i++)
        {
            normalizedRoles[i] = NormalizeName(roles[i])!;
        }

        var rolesIds = await _context
            .Roles
            .Where(p => normalizedRoles.Contains(p.NormalizedName))
            .Select(p => p.Id)
            .ToArrayAsync()
            .ConfigureAwait(false);

        if (rolesIds.Length != roles.Length)
        {
            throw new InvalidOperationException("One or more roles were not found.");
        }

        var userRoles = new IdentityUserRole<int>[rolesIds.Length];
        for (var i = 0; i < rolesIds.Length; i++)
        {
            userRoles[i] = new IdentityUserRole<int>
            {
                UserId = user.Id,
                RoleId = rolesIds[i],
            };
        }

        await _context.UserRoles.AddRangeAsync(userRoles).ConfigureAwait(false);
    }

    private async Task AddUserClaimsAsync(TUser user, Claim[] claims)
    {
        var userClaims = new IdentityUserClaim<int>[claims.Length];
        for (var i = 0; i < claims.Length; i++)
        {
            var claim = claims[i];

            userClaims[i] = new IdentityUserClaim<int>
            {
                UserId = user.Id,
                ClaimType = claim.Type,
                ClaimValue = claim.Value,
            };
        }

        await _context.UserClaims.AddRangeAsync(userClaims).ConfigureAwait(false);
    }

    private async Task SaveChangesAsync(bool autoSave)
    {
        if (autoSave)
        {
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
