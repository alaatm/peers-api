namespace Mashkoor.Modules.Users.Events;

public sealed class AppOpened : TraceableNotification
{
    public int UserId { get; }
    public DateTime Date { get; }

    public AppOpened(
        DateTime date,
        IIdentityInfo identityInfo,
        int userId) : base(identityInfo)
    {
        UserId = userId;
        Date = date;
    }
}
