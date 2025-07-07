using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Mashkoor.Core.Data;
using Mashkoor.Core.Identity;

namespace Mashkoor.Core.Test;

public static class SharedStubs
{
    public static HttpContext BuildHttpContext(string ip, int? id = null, string username = null, string[] roles = null, Claim[] claims = null, string traceId = null, IDictionary contextItems = null)
    {
        username = username?.Trim();

        // If username is null or empty, id should be null.
        if (id.HasValue)
        {
            Debug.Assert(!string.IsNullOrEmpty(username), "When id is provided, username must be non-null and non-empty.");
        }

        // If username is provided (non-empty), id must be provided.
        if (!string.IsNullOrEmpty(username))
        {
            Debug.Assert(id.HasValue, "When username is provided, id must be provided.");
        }

        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = ip is { Length: > 0 } ? IPAddress.Parse(ip) : null;
        httpContext.TraceIdentifier = traceId;

        if (username is { Length: > 0 })
        {
            var identity = new GenericIdentity(username);
            identity.AddClaims(claims ?? []);
            identity.AddClaim(new Claim(CustomClaimTypes.Id, id.Value!.ToString(CultureInfo.InvariantCulture)));
            var principal = new GenericPrincipal(identity, roles ?? []);

            httpContext.User = principal;
        }
        else
        {
            httpContext.User = new ClaimsPrincipal();
        }

        if (contextItems is not null)
        {
            foreach (DictionaryEntry item in contextItems)
            {
                httpContext.Items.Add(item.Key, item.Value);
            }
        }

        return httpContext;
    }
}

public class TestContext : DbContextBase<TestUser>
{
    public bool CreatedViaFactory { get; set; }
    public TestContext(DbContextOptions options)
        : base(options) { }
}

public class TestContextFactory : IDbContextFactory<TestContext>
{
    private readonly IDbContextFactory<TestContext> _pooledFactory;

    public TestContextFactory(
        IDbContextFactory<TestContext> pooledFactory) => _pooledFactory = pooledFactory;

    public TestContext CreateDbContext()
    {
        var context = _pooledFactory.CreateDbContext();
        context.CreatedViaFactory = true;
        return context;
    }
}

public class TestUser : IdentityUserBase
{
}

public static class MockBuilder
{
    public static Mock<IStringLocalizerFactory> GetLocalizerFactoryMoq()
    {
        var stringLocalizerFactory = new Mock<IStringLocalizerFactory>();
        stringLocalizerFactory
            .Setup(s => s.Create(It.IsAny<Type>()))
            .Returns((Type t) => new SLMoq());
        return stringLocalizerFactory;
    }

    public class SLMoq : IStringLocalizer
    {
        public virtual LocalizedString this[string name]
            => new(name, name);
        public virtual LocalizedString this[string name, params object[] arguments]
            => new(string.Format(CultureInfo.InvariantCulture, name, arguments), string.Format(CultureInfo.InvariantCulture, name, arguments));
        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
            => throw new NotImplementedException();
    }

    public class SLMoq<T> : SLMoq, IStringLocalizer<T>
    {
    }

    public class SLCultureMoq<T> : SLMoq, IStringLocalizer<T>
    {
        public override LocalizedString this[string name]
        {
            get
            {
                var l = base[name];
                return new(l.Name, $"{Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName}:{l.Value}");
            }
        }

        public override LocalizedString this[string name, params object[] arguments]
        {
            get
            {
                var l = base[name, arguments];
                return new(l.Name, $"{Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName}:{l.Value}");
            }
        }
    }
}
