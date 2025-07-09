using Mashkoor.Modules.Users.Domain;

namespace Mashkoor.Modules.Kernel.Pipelines;

internal class IdentityCheckBehaviorOptions
{
#pragma warning disable IDE1006
    // This is only used for tests so that it can be disabled for integration tests.
    internal static bool Enabled = true;
#pragma warning restore IDE1006
}

/// <summary>
/// Ensures that an authenticated identity exist in the database.
/// </summary>
public sealed class IdentityCheckBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommand, IRequest<TResponse>
    where TResponse : IResult
{
    private static readonly Func<MashkoorContext, int?, Task<UserStatus>> _getUserStatus
        = EF.CompileAsyncQuery(
            (MashkoorContext context, int? userId)
            => context.Users
                .Where(b => b.Id == userId)
                .Select(p => p.Status)
                .FirstOrDefault());

    // Used for resolving ShiftleeContext only when receiving authenticated requests.
    // For non-authenticated requests, the HttpContext maybe missing tenant information which will make constructing the context (via ctor DI) impossible.
    private readonly IServiceProvider _services;
    private readonly IIdentityInfo _identity;
    private readonly ILogger<IdentityCheckBehavior<TRequest, TResponse>> _log;
    private readonly IStrLoc _l;

    public IdentityCheckBehavior(
        IServiceProvider services,
        IIdentityInfo identity,
        ILogger<IdentityCheckBehavior<TRequest, TResponse>> log,
        IStrLoc l)
    {
        _services = services;
        _identity = identity;
        _log = log;
        _l = l;
    }

    public async Task<TResponse> Handle(TRequest cmd, [NotNull] RequestHandlerDelegate<TResponse> next, CancellationToken ctk = default)
    {
        if (IdentityCheckBehaviorOptions.Enabled &&
            _identity.IsAuthenticated)
        {
            var context = _services.GetRequiredService<MashkoorContext>();
            var status = await _getUserStatus(context, _identity.Id);

            if (status == UserStatus.Banned)
            {
                _log.BannedUserActivity(_identity.Username, cmd.GetType());
                return (TResponse)Result.Forbidden(_l["Access is forbidden."], type: "USER_BANNED");
            }
            else if (status == UserStatus.None)
            {
                // UserStatus.None is the default value for the enum which means that the user does not exist.
                _log.AuthenticatedUserNotFound(_identity.Username);
                return (TResponse)Result.Problem("Unexpected state.", statusCode: 500);
            }
        }

        return await next(ctk);
    }
}
