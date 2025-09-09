using Peers.Core.Background;

namespace Peers.Modules.Kernel;

/// <summary>
/// Represents a db context factory to provide pooled instances. Instances of <see cref="PeersContext"/>
/// are registered as scoped that get created via the CreateDbContext method below, thus providing auto disposal
/// once scope is out. The registration and setup of pooling is defined in
/// <see cref="Core.Data.ServiceCollectionExtensions.AddDataServices{TContext, TContextFactory, TUser}(IServiceCollection, Action{DbContextOptionsBuilder})"/>
/// </summary>
public sealed class PeersContextScopedFactory : IDbContextFactory<PeersContext>
{
    private readonly IDbContextFactory<PeersContext> _pooledFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IProducer _producer;

    /// <summary>
    /// Initializes a new instance of <see cref="PeersContextScopedFactory"/>.
    /// </summary>
    /// <param name="pooledFactory">The pooled factory singleton.</param>
    /// <param name="httpContextAccessor">The http context accessor.</param>
    /// <param name="producer">The producer singleton.</param>
    public PeersContextScopedFactory(
        IDbContextFactory<PeersContext> pooledFactory,
        IHttpContextAccessor httpContextAccessor,
        IProducer producer)
    {
        _pooledFactory = pooledFactory;
        _httpContextAccessor = httpContextAccessor;
        _producer = producer;
    }

    /// <summary>
    /// Creates a new instance of <see cref="PeersContext"/>.
    /// </summary>
    public PeersContext CreateDbContext()
    {
        var context = _pooledFactory.CreateDbContext();
        context.TraceIdentifier = _httpContextAccessor.HttpContext?.TraceIdentifier;
        context.Producer = _producer;
        return context;
    }
}
