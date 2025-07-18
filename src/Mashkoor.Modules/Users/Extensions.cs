using Mashkoor.Core.Communication;
using Mashkoor.Modules.Users.Domain;

namespace Mashkoor.Modules.Users;

public static class AppUserQueryableExtensions
{
    extension(IQueryable<AppUser> q)
    {
        public IQueryable<SimpleUser> ProjectToSimpleUser() => q.Select(p => new SimpleUser
        {
            PreferredLanguage = p.PreferredLanguage,
            UserHandles = p.DeviceList.Select(p => p.PnsHandle),
            Email = p.Email,
        });
    }
}

public static class AppUserExtensions
{
    extension(AppUser appUser)
    {
        public SimpleUser ToSimpleUser() => new()
        {
            PreferredLanguage = appUser.PreferredLanguage,
            UserHandles = appUser.DeviceList.Select(p => p.PnsHandle),
            Email = appUser.Email,
        };
    }
}
