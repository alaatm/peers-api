using Mashkoor.Core.Background;

namespace Mashkoor.Modules.Kernel;

/// <summary>
/// Represents a db context factory to provide pooled instances. Instances of <see cref="MashkoorContext"/>
/// are registered as scoped that get created via the CreateDbContext method below, thus providing auto disposal
/// once scope is out. The registration and setup of pooling is defined in
/// <see cref="Core.Data.ServiceCollectionExtensions.AddDataServices{TContext, TContextFactory, TUser}(IServiceCollection, Action{DbContextOptionsBuilder})"/>
/// </summary>
public sealed class MashkoorContextScopedFactory : IDbContextFactory<MashkoorContext>
{
    private readonly IDbContextFactory<MashkoorContext> _pooledFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IProducer _producer;

    /// <summary>
    /// Initializes a new instance of <see cref="MashkoorContextScopedFactory"/>.
    /// </summary>
    /// <param name="pooledFactory">The pooled factory singleton.</param>
    /// <param name="httpContextAccessor">The http context accessor.</param>
    /// <param name="producer">The producer singleton.</param>
    public MashkoorContextScopedFactory(
        IDbContextFactory<MashkoorContext> pooledFactory,
        IHttpContextAccessor httpContextAccessor,
        IProducer producer)
    {
        _pooledFactory = pooledFactory;
        _httpContextAccessor = httpContextAccessor;
        _producer = producer;
    }

    /// <summary>
    /// Creates a new instance of <see cref="MashkoorContext"/>.
    /// </summary>
    public MashkoorContext CreateDbContext()
    {
        var context = _pooledFactory.CreateDbContext();
        context.TraceIdentifier = _httpContextAccessor.HttpContext?.TraceIdentifier;
        context.Producer = _producer;
        return context;
    }
}
