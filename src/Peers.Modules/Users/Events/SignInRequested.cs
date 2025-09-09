namespace Peers.Modules.Users.Events;

public sealed class SignInRequested : EnrollRequested
{
    public string Platform { get; set; }

    public SignInRequested(
        IIdentityInfo identityInfo,
        string platform,
        string username,
        string langCode) : base(identityInfo, username, langCode)
        => Platform = platform;
}
