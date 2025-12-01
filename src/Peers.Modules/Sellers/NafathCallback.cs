using System.Diagnostics;
using Peers.Core.Communication;
using Peers.Core.Communication.Push;
using Peers.Core.Nafath.Models;
using Peers.Modules.Sellers.Domain;
using Peers.Modules.Users;
using Peers.Modules.Users.Domain;

namespace Peers.Modules.Sellers;

public static class NafathCallback
{
    public static async Task Handler(
        IServiceProvider services,
        int userId,
        NafathIdentity? nafathIdentity)
    {
        var l = services.GetRequiredService<IStrLoc>();
        var context = services.GetRequiredService<PeersContext>();
        var push = services.GetRequiredService<IPushNotificationService>();
        var timeProvider = services.GetRequiredService<TimeProvider>();

        var user = await context.Users
            .Include(p => p.DeviceList)
            .Where(p => p.Id == userId)
            .FirstAsync();

        var success = nafathIdentity is not null;
        var data = new Dictionary<string, string>
        {
            { "event", "nafath-enrollment" },
            { "success", success ? "1" : "0" },
        };

        var notification = MessageBuilder.Create(l).Add(user.ToSimpleUser(), data);

        if (success)
        {
            Debug.Assert(nafathIdentity is not null);

            var um = services.GetRequiredService<UserManager<AppUser>>();
            if (await um.AddToRoleAsync(user, Roles.Seller) is { Succeeded: true })
            {
                context.Sellers.Add(Seller.Create(user, nafathIdentity, timeProvider.UtcNow()));
                await context.SaveChangesAsync();
                await push.DispatchAsync(notification.Generate());
            }
        }
        else
        {
            await push.DispatchAsync(notification.Generate());
        }
    }
}
