namespace Peers.Modules.Users.Events;

public class EnrollRequested : TraceableNotification
{
    public string Username { get; }
    public string PhoneNumber { get; }
    public string LangCode { get; }

    public EnrollRequested(
        IIdentityInfo identityInfo,
        string username,
        string phoneNumber,
        string langCode) : base(identityInfo)
    {
        Username = username;
        PhoneNumber = phoneNumber;
        LangCode = langCode;
    }
}
