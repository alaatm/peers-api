using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Diagnostics.CodeAnalysis;

namespace Mashkoor.Core.Data.Identity;

/// <summary>
/// Provides methods for managing roles in a persistence store.
/// </summary>
/// <typeparam name="TUser">The type representing a user.</typeparam>
/// <typeparam name="TContext">The type of the data context class used to access the persistence store.</typeparam>
public sealed class IdentityRoleManager<TUser, TContext> : RoleManager<IdentityRole<int>>
    where TUser : IdentityUser<int>
    where TContext : IdentityDbContext<TUser, IdentityRole<int>, int>
{
    private readonly TContext _context;

    public IdentityRoleManager(
        TContext context,
        IRoleStore<IdentityRole<int>> store,
        ILookupNormalizer lookupNormalizer,
        IdentityErrorDescriber errors,
        ILogger<RoleManager<IdentityRole<int>>> logger) : base(store, [], lookupNormalizer, errors, logger)
        => _context = context;

    /// <summary>
    /// Creates the specified roles in the persistence store.
    /// </summary>
    /// <param name="autoSave">Whether to automatically save changes to the persistence store.</param>
    /// <param name="roles">The roles to create.</param>
    /// <returns></returns>
    public async Task CreateRolesAsync(
        bool autoSave,
        [NotNull] params string[] roles)
    {
        ((RoleStore<IdentityRole<int>, TContext, int>)Store).AutoSaveChanges = false;

        var identityRoles = new IdentityRole<int>[roles.Length];
        for (var i = 0; i < roles.Length; i++)
        {
            var roleName = roles[i];
            await UpdateNormalizedRoleNameAsync(identityRoles[i] = new IdentityRole<int>(roleName)).ConfigureAwait(false);
        }

        await _context.Roles.AddRangeAsync(identityRoles).ConfigureAwait(false);
        await SaveChangesAsync(autoSave).ConfigureAwait(false);
    }

    public override Task<IdentityResult> CreateAsync(IdentityRole<int> role) => throw new NotSupportedException();

    private async Task SaveChangesAsync(bool autoSave)
    {
        if (autoSave)
        {
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
