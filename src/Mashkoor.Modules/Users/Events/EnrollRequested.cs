namespace Mashkoor.Modules.Users.Events;

public sealed class EnrollRequested : TraceableNotification
{
    public string Username { get; }
    public string LangCode { get; }

    public EnrollRequested(
        IIdentityInfo identityInfo,
        string username,
        string langCode) : base(identityInfo)
    {
        Username = username;
        LangCode = langCode;
    }
}
