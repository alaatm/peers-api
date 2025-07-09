using Mashkoor.Core.Domain.Rules;
using Mashkoor.Modules.Users.Domain;

namespace Mashkoor.Modules.Users.Rules;

public sealed class CanLinkDeviceRule : BusinessRule
{
    private readonly IStringLocalizer _l;
    private readonly AppUser _user;
    private readonly Device _device;

    public override string ErrorTitle => _l["Error linking device"];

    public CanLinkDeviceRule(
        AppUser user,
        Device device)
    {
        _l = StringLocalizerFactory.Create(typeof(res));
        _user = user;
        _device = device;
    }

    public override bool IsBroken()
    {
        if (_user.DeviceList.Any(p => p.DeviceId == _device.DeviceId))
        {
            Append(_l["The device is already linked."]);
            return true;
        }

        return Errors.Any();
    }
}
