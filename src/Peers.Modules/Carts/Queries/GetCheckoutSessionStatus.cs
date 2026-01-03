using Peers.Modules.Carts.Domain;

namespace Peers.Modules.Carts.Queries;

public static class GetCheckoutSessionStatus
{
    public const string EndpointName = "GetCheckoutSessionStatus";

    /// <summary>
    /// Retrieves the status of a checkout session by its unique session identifier.
    /// </summary>
    /// <param name="SessionId">The unique identifier of the checkout session.</param>
    [Authorize(Roles = Roles.Customer)]
    public sealed record Query(Guid SessionId) : IQuery;

    /// <summary>
    /// Represents the result of a checkout session, including its status and the associated order identifier, if
    /// available.
    /// </summary>
    /// <param name="Status">The current status of the checkout session.</param>
    /// <param name="OrderId">The unique identifier of the order associated with the session, or null if no order has been created.</param>
    public sealed record Response(
        CheckoutSessionStatus Status,
        int? OrderId);

    public sealed class Handler : ICommandHandler<Query>
    {
        private readonly PeersContext _context;
        private readonly IIdentityInfo _identity;

        public Handler(
            PeersContext context,
            IIdentityInfo identity)
        {
            _context = context;
            _identity = identity;
        }
        public async Task<IResult> Handle([NotNull] Query cmd, CancellationToken ctk)
        {
            var session = await _context.CheckoutSessions
                .Where(p => p.SessionId == cmd.SessionId && p.CustomerId == _identity.Id)
                .Select(p => new
                {
                    p.Id,
                    p.Status,
                })
                .FirstOrDefaultAsync(ctk);

            return session is not null
                ? Result.Ok(new Response(session.Status, session.Status is CheckoutSessionStatus.Completed ? session.Id : null))
                : Result.NotFound();
        }
    }
}
