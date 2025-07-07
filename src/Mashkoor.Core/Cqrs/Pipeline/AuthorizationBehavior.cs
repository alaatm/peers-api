using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Mashkoor.Core.Commands;
using Mashkoor.Core.Http;
using Mashkoor.Core.Identity;

namespace Mashkoor.Core.Cqrs.Pipeline;

public sealed class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommand, IRequest<TResponse>
    where TResponse : IResult
{
    private static readonly ConcurrentDictionary<Type, string[][]> _cmdAuthCache = [];
    private readonly IIdentityInfo _identity;
    private readonly IStrLoc _l;

    public AuthorizationBehavior(
        IIdentityInfo identity,
        IStrLoc l)
    {
        _identity = identity;
        _l = l;
    }

    public Task<TResponse> Handle(TRequest cmd, [NotNull] RequestHandlerDelegate<TResponse> next, CancellationToken ctk = default)
    {
        var authAttributes = GetEntry(cmd.GetType());
        if (authAttributes.Length > 0)
        {
            // Must be authenticated
            if (!_identity.IsAuthenticated)
            {
                return Task.FromResult((TResponse)Result.Unauthorized(detail: _l["Authentication required."], type: "AUTH_REQUIRED"));
            }

            foreach (var authAttr in authAttributes)
            {
                var authorized = false;

                // Role-based authorization
                var roles = authAttr;
                if (roles.Length > 0)
                {
                    // User needs to be in atleast one rule
                    foreach (var role in roles)
                    {
                        if (_identity.IsInRole(role))
                        {
                            authorized = true;
                            break;
                        }
                    }

                    if (!authorized)
                    {
                        return Task.FromResult((TResponse)Result.AccessRestricted(_l["You are not authorized to perform this operation."]));
                    }
                }
            }
        }

        return next(ctk);
    }

    private static string[][] GetEntry(Type cmdType)
    {
        if (_cmdAuthCache.TryGetValue(cmdType, out var entry))
        {
            return entry;
        }

        var authAttributes = Attribute.GetCustomAttributes(cmdType, typeof(AuthorizeAttribute));
        var arr = new string[authAttributes.Length][];

        for (var i = 0; i < authAttributes.Length; i++)
        {
            arr[i] = ExtractRoles((AuthorizeAttribute)authAttributes[i]);
        }

        _cmdAuthCache[cmdType] = arr;
        return arr;
    }

    private static string[] ExtractRoles(AuthorizeAttribute authAttr)
    {
        var roles = authAttr.Roles?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? [];
        if (roles.Length > 0)
        {
            var c = 0;
            var retVal = new string[roles.Length];

            foreach (var role in roles)
            {
                var r = role.Trim();
                if (r.Length > 0)
                {
                    retVal[c++] = r;
                }
            }

            return retVal[..c];
        }

        return [];
    }
}
