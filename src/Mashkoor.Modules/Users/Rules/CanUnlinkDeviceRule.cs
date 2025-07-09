using Mashkoor.Core.Domain.Rules;
using Mashkoor.Modules.Users.Domain;

namespace Mashkoor.Modules.Users.Rules;

public sealed class CanUnlinkDeviceRule : BusinessRule
{
    private readonly IStringLocalizer _l;
    private readonly AppUser _user;
    private readonly Device _device;

    public override string ErrorTitle => _l["Error unlinking device"];

    public CanUnlinkDeviceRule(
        AppUser user,
        Device device)
    {
        _l = StringLocalizerFactory.Create(typeof(res));
        _user = user;
        _device = device;
    }

    public override bool IsBroken()
    {
        var userDevice = _user.DeviceList.SingleOrDefault(p => p.DeviceId == _device.DeviceId);

        if (userDevice is null)
        {
            Append(_l["Cannot unlink an unlinked device."]);
            return true;
        }

        // Verify that all device props match
        if (userDevice.Manufacturer != _device.Manufacturer ||
            userDevice.Model != _device.Model ||
            userDevice.Platform != _device.Platform ||
            userDevice.OSVersion != _device.OSVersion ||
            userDevice.Idiom != _device.Idiom ||
            userDevice.DeviceType != _device.DeviceType)
        {
            Append(_l["The device to be unlinked is found with matching id but the device props differ."]);
            return true;
        }

        return Errors.Any();
    }
}
