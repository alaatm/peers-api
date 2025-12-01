using System.Diagnostics;

namespace Peers.Modules.Users.Queries;

public static class GetProfile
{
    [Authorize(Roles = $"{Roles.Customer}, {Roles.UsersManager}")]
    public sealed record Query(int? Id) : IQuery;

    public sealed record Response(
        string? Firstname,
        string? Lastname,
        string Phone,
        string? Email,
        bool IsVerifiedEmail,
        string PreferredLanguage);

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
            if (cmd.Id.HasValue && _identity.IsCustomer)
            {
                return Result.BadRequest("Use '/users/me/profile' for querying own profile.");
            }

            Debug.Assert(
                (cmd.Id.HasValue && _identity.IsStaff) ||
                (!cmd.Id.HasValue && _identity.IsCustomer));

            var q = cmd.Id.HasValue
                ? _context.Users.Where(p => p.Id == cmd.Id)
                : _context.Users.Where(p => p.Id == _identity.Id);

            var r = await q
                .Select(p => new Response(
                    p.Firstname,
                    p.Lastname,
                    p.PhoneNumber!,
                    p.UpdatedEmail ?? p.Email,
                    p.EmailConfirmed,
                    p.PreferredLanguage))
                .FirstOrDefaultAsync(ctk);

            return r is not null
                ? Result.Ok(r)
                : Result.NotFound();
        }
    }
}
