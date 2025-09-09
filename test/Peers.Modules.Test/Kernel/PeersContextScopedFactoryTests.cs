using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Peers.Core.Background;

namespace Peers.Modules.Test.Kernel;

public class PeersContextScopedFactoryTests
{
    private static readonly string _testConnStr = TestConfig.GetConnectionString("integration", "ConnStrIntegration");

    [Fact]
    public void CreateDbContext_sets_TraceIdentifier_on_the_created_context()
    {
        // Arrange
        var traceIdentifier = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder().UseSqlServer(_testConnStr);

        var factoryMoq = new Mock<IDbContextFactory<PeersContext>>(MockBehavior.Strict);
        factoryMoq.Setup(p => p.CreateDbContext()).Returns(new PeersContext(options.Options));
        var httpContextAccessorMoq = new Mock<IHttpContextAccessor>();
        httpContextAccessorMoq.SetupGet(p => p.HttpContext).Returns(new DefaultHttpContext() { TraceIdentifier = traceIdentifier });
        var factory = new PeersContextScopedFactory(factoryMoq.Object, httpContextAccessorMoq.Object, Mock.Of<IProducer>());

        // Act
        var context = factory.CreateDbContext();

        // Assert
        Assert.Equal(traceIdentifier, context.TraceIdentifier);
        factoryMoq.VerifyAll();
        httpContextAccessorMoq.VerifyAll();
    }

    [Fact]
    public void CreateDbContext_sets_producer_on_the_created_context()
    {
        // Arrange
        var options = new DbContextOptionsBuilder().UseSqlServer(_testConnStr);

        var producer = Mock.Of<IProducer>();
        var factoryMoq = new Mock<IDbContextFactory<PeersContext>>(MockBehavior.Strict);
        factoryMoq.Setup(p => p.CreateDbContext()).Returns(new PeersContext(options.Options));
        var factory = new PeersContextScopedFactory(factoryMoq.Object, Mock.Of<IHttpContextAccessor>(), producer);

        // Act
        var context = factory.CreateDbContext();

        // Assert
        Assert.Same(producer, context.Producer);
        factoryMoq.VerifyAll();
    }

    [Fact]
    public void CreateDbContext_handles_null_HttpContext()
    {
        var traceIdentifier = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder().UseSqlServer(_testConnStr);

        var factoryMoq = new Mock<IDbContextFactory<PeersContext>>(MockBehavior.Strict);
        factoryMoq.Setup(p => p.CreateDbContext()).Returns(new PeersContext(options.Options));
        var httpContextAccessorMoq = new Mock<IHttpContextAccessor>();
        httpContextAccessorMoq.SetupGet(p => p.HttpContext).Returns((HttpContext)null);
        var factory = new PeersContextScopedFactory(factoryMoq.Object, httpContextAccessorMoq.Object, Mock.Of<IProducer>());

        // Act
        var context = factory.CreateDbContext();

        // Assert
        Assert.Null(context.TraceIdentifier);
        factoryMoq.VerifyAll();
        httpContextAccessorMoq.VerifyAll();
    }
}
