namespace Peers.Modules.Carts.Events;

public sealed class ApiPaymentIntentIssued : TraceableNotification
{
    public int UserId { get; }
    public Guid SessionId { get; }

    public ApiPaymentIntentIssued(
        Guid sessionId,
        IIdentityInfo identityInfo) : base(identityInfo)
    {
        UserId = identityInfo.Id;
        SessionId = sessionId;
    }
}
