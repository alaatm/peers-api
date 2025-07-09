using Mashkoor.Core.Communication;
using Mashkoor.Modules.Users.Domain;

namespace Mashkoor.Modules.Users;

public static class AppUserQuerableExtensions
{
    public static IQueryable<SimpleUser> ProjectToSimpleUser(this IQueryable<AppUser> q) =>
        q.Select(p => new SimpleUser
        {
            PreferredLanguage = p.PreferredLanguage,
            UserHandles = p.DeviceList.Select(p => p.PnsHandle),
            Email = p.Email,
        });
}

public static class AppUserExtensions
{
    public static SimpleUser ToSimpleUser([NotNull] this AppUser appUser) =>
        new()
        {
            PreferredLanguage = appUser.PreferredLanguage,
            UserHandles = appUser.DeviceList.Select(p => p.PnsHandle),
            Email = appUser.Email,
        };
}
