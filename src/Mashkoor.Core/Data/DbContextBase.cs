using System.Diagnostics.CodeAnalysis;
using Humanizer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mashkoor.Core.Background;
using Mashkoor.Core.Data.ValueConverters;
using Mashkoor.Core.Domain;

namespace Mashkoor.Core.Data;

public abstract class DbContextBase<TUser> : IdentityDbContext<TUser, IdentityRole<int>, int>
    where TUser : IdentityUserBase
{
    public IProducer? Producer { get; set; }
    public string? TraceIdentifier { get; set; }

    /// <inheritdoc />
    protected DbContextBase(DbContextOptions options) : base(options)
    {
    }

    /// <inheritdoc />
    protected override void OnModelCreating([NotNull] ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<IdentityRoleClaim<int>>(BuildModel);
        builder.Entity<IdentityRole<int>>(BuildModel);
        builder.Entity<IdentityUserClaim<int>>(BuildModel);
        builder.Entity<IdentityUserLogin<int>>(BuildModel);
        builder.Entity<IdentityUserRole<int>>(BuildModel);
        builder.Entity<IdentityUserToken<int>>(BuildModel);
        builder.Entity<TUser>(BuildModel);

        builder.Entity<IdentityRoleClaim<int>>().Property(p => p.ClaimValue).Metadata.RemoveAnnotation("MaxLength");
        builder.Entity<IdentityUserClaim<int>>().Property(p => p.ClaimValue).Metadata.RemoveAnnotation("MaxLength");
        builder.Entity<TUser>().Property(p => p.PasswordHash).Metadata.RemoveAnnotation("MaxLength");
        builder.Entity<TUser>().Property(p => p.SecurityStamp).Metadata.RemoveAnnotation("MaxLength");

        // Restore original max length for IdentityUserLogin & IdentityUserToken due to StrMaxLen migration failing
        builder.Entity<IdentityUserLogin<int>>().Property(p => p.LoginProvider).Metadata.RemoveAnnotation("MaxLength");
        builder.Entity<IdentityUserLogin<int>>().Property(p => p.ProviderKey).Metadata.RemoveAnnotation("MaxLength");
        builder.Entity<IdentityUserLogin<int>>().Property(p => p.ProviderDisplayName).Metadata.RemoveAnnotation("MaxLength");
        builder.Entity<IdentityUserToken<int>>().Property(p => p.LoginProvider).Metadata.RemoveAnnotation("MaxLength");
        builder.Entity<IdentityUserToken<int>>().Property(p => p.Name).Metadata.RemoveAnnotation("MaxLength");
        builder.Entity<IdentityUserToken<int>>().Property(p => p.Value).Metadata.RemoveAnnotation("MaxLength");

        static void BuildModel<TEntity>(EntityTypeBuilder<TEntity> builder) where TEntity : class => builder
            .ToTable(typeof(TEntity).Name
            .Replace("Identity", "", StringComparison.OrdinalIgnoreCase)
            .Replace("`1", "", StringComparison.OrdinalIgnoreCase).Underscore(), "id");
    }

    /// <inheritdoc />
    protected override void ConfigureConventions([NotNull] ModelConfigurationBuilder builder)
    {
        builder.Properties<string>().HaveMaxLength(256);
        builder.Properties<Uri>().HaveMaxLength(256);
        builder.Properties<decimal>().HavePrecision(18, 2);
        builder.Properties<DateTime>().HaveConversion<DateTimeUTCConverter>();
        builder.Properties<DateTime?>().HaveConversion<NullableDateTimeUTCConverter>();
        builder.Conventions.Add(_ => new SnakeCaseNameRewriter());
    }

    /// <inheritdoc/>
    public override Task<int> SaveChangesAsync(CancellationToken ctk = default)
        => SaveChangesAsync(deferEventPublishing: false, 0, ctk);

    private readonly List<DomainEvent> _deferredEvents = [];

    /// <summary>
    /// Saves all changes made in this context to the database. The domain events publishing can be deferred.
    /// </summary>
    /// <param name="deferEventPublishing">Indicates whether to defer domain events publishing.</param>
    /// <param name="_">Not used</param>
    /// <param name="ctk">The cancellation token.</param>
    /// <returns></returns>
    public async Task<int> SaveChangesAsync(bool deferEventPublishing, int _, CancellationToken ctk = default)
    {
        var result = await base.SaveChangesAsync(ctk);

        if (Producer is not null)
        {
            foreach (var @event in GetAllEvents())
            {
                @event.TraceIdentifier ??= TraceIdentifier;

                if (deferEventPublishing)
                {
                    _deferredEvents.Add(@event);
                }
                else
                {
                    await Producer.PublishAsync(@event, ctk);
                }
            }
        }

        return result;

        List<DomainEvent> GetAllEvents()
        {
            var events = new List<DomainEvent>();

            foreach (var e in ChangeTracker.Entries<IEntity>())
            {
                events.AddRange(e.Entity.GetEvents(true));
            }

            return events;
        }
    }

    /// <summary>
    /// Publishes all deferred domain events.
    /// </summary>
    /// <param name="ctk"></param>
    /// <returns></returns>
    public async Task PublishDeferredEvents(CancellationToken ctk = default)
    {
        var events = _deferredEvents.ToArray();
        _deferredEvents.Clear();

        foreach (var @event in events)
        {
            await Producer!.PublishAsync(@event, ctk);
        }
    }

    public override int SaveChanges() => SaveChangesAsync().GetAwaiter().GetResult();
}
