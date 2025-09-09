using Humanizer;
using Peers.Core.Domain.Rules;
using Peers.Modules.Users.Domain;

namespace Peers.Modules.Users.Rules;

public sealed class CanChangeUserStatusRule : BusinessRule
{
    private readonly IStringLocalizer _l;
    private readonly AppUser _user;
    private readonly UserStatus _newStatus;

    public override string ErrorTitle => _l["Error changing user status"];

    public CanChangeUserStatusRule(
        AppUser user,
        UserStatus newStatus)
    {
        _l = StringLocalizerFactory.Create(typeof(res));
        _user = user;
        _newStatus = newStatus;
    }

    public override bool IsBroken()
    {
        if (_user.Status is UserStatus.Deleted)
        {
            if (_newStatus != UserStatus.Deleted)
            {
                Append(_l["Account is deleted."]);
                return true;
            }
        }
        else
        {
            if (_newStatus == UserStatus.None)
            {
                Append(_l["The status '{0}' cannot be used.", _l[$"{_newStatus.Humanize()}"].Value]);
                return true;
            }

            if (_user.Status == _newStatus)
            {
                Append(_l["The current user status is already set to '{0}'.", _l[$"{_newStatus.Humanize()}"].Value]);
                return true;
            }
        }

        return Errors.Any();
    }
}
