namespace Peers.Modules.Users.Events;

public sealed class SignInRequested : TraceableNotification
{
    public string? Username { get; set; }
    public string? PhoneNumber { get; }
    public string LangCode { get; }
    public string Platform { get; set; }

    public SignInRequested(
        IIdentityInfo identityInfo,
        string platform,
        string? username,
        string? phoneNumber,
        string langCode) : base(identityInfo)
    {
        Username = username;
        PhoneNumber = phoneNumber;
        LangCode = langCode;
        Platform = platform;
    }
}
