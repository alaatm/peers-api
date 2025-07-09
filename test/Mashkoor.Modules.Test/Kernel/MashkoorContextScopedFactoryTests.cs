using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Mashkoor.Core.Background;

namespace Mashkoor.Modules.Test.Kernel;

public class MashkoorContextScopedFactoryTests
{
    private static readonly string _testConnStr = TestConfig.GetConnectionString("integration", "ConnStrIntegration");

    [Fact]
    public void CreateDbContext_sets_TraceIdentifier_on_the_created_context()
    {
        // Arrange
        var traceIdentifier = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder().UseSqlServer(_testConnStr);

        var factoryMoq = new Mock<IDbContextFactory<MashkoorContext>>(MockBehavior.Strict);
        factoryMoq.Setup(p => p.CreateDbContext()).Returns(new MashkoorContext(options.Options));
        var httpContextAccessorMoq = new Mock<IHttpContextAccessor>();
        httpContextAccessorMoq.SetupGet(p => p.HttpContext).Returns(new DefaultHttpContext() { TraceIdentifier = traceIdentifier });
        var factory = new MashkoorContextScopedFactory(factoryMoq.Object, httpContextAccessorMoq.Object, Mock.Of<IProducer>());

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
        var factoryMoq = new Mock<IDbContextFactory<MashkoorContext>>(MockBehavior.Strict);
        factoryMoq.Setup(p => p.CreateDbContext()).Returns(new MashkoorContext(options.Options));
        var factory = new MashkoorContextScopedFactory(factoryMoq.Object, Mock.Of<IHttpContextAccessor>(), producer);

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

        var factoryMoq = new Mock<IDbContextFactory<MashkoorContext>>(MockBehavior.Strict);
        factoryMoq.Setup(p => p.CreateDbContext()).Returns(new MashkoorContext(options.Options));
        var httpContextAccessorMoq = new Mock<IHttpContextAccessor>();
        httpContextAccessorMoq.SetupGet(p => p.HttpContext).Returns((HttpContext)null);
        var factory = new MashkoorContextScopedFactory(factoryMoq.Object, httpContextAccessorMoq.Object, Mock.Of<IProducer>());

        // Act
        var context = factory.CreateDbContext();

        // Assert
        Assert.Null(context.TraceIdentifier);
        factoryMoq.VerifyAll();
        httpContextAccessorMoq.VerifyAll();
    }
}
