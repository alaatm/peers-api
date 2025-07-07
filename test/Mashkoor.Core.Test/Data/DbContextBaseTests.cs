using Microsoft.EntityFrameworkCore;
using Mashkoor.Core.Background;
using Mashkoor.Core.Domain;

namespace Mashkoor.Core.Test.Data;

public class DbContextBaseTests
{
    [Fact]
    public async Task SaveChanges_does_not_dispatch_domain_events_when_dispatcher_is_not_set()
    {
        // Arrange
        using var context = new MyContext(null);
        var conn = context.Database.GetDbConnection();
        conn.Open();
        context.Database.EnsureCreated();

        var blog = new Blog { RequiredProp = "test" };
        blog.AddTestEvent();
        context.Blogs.Add(blog);

        // Act
        await context.SaveChangesAsync();

        // Assert
        Assert.Single(context.Blogs);
        Assert.Single(blog.GetEvents());
    }

    [Fact]
    public async Task SaveChanges_does_not_dispatch_domain_events_when_save_is_not_successful()
    {
        // Arrange
        var producerMoq = new Mock<IProducer>(MockBehavior.Strict);

        using var context = new MyContext(producerMoq.Object);
        var conn = context.Database.GetDbConnection();
        conn.Open();
        context.Database.EnsureCreated();

        var blog = new Blog();
        blog.AddTestEvent();
        context.Blogs.Add(blog);

        // Act
        try
        {
            await context.SaveChangesAsync();
        }
        catch { }

        // Assert
        Assert.Empty(context.Blogs);
        Assert.Single(blog.GetEvents());
        producerMoq.Verify(
            p => p.PublishAsync(It.IsAny<TestEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task SaveChanges_sets_traceIdentifier_on_the_message_before_dispatch()
    {
        // Arrange
        var traceId = "trace-id";

        var testEvent = new TestEvent();
        var producerMoq = new Mock<IProducer>(MockBehavior.Strict);
        producerMoq
            .Setup(p => p.PublishAsync(It.Is<ITraceableNotification>(p => p.TraceIdentifier == traceId), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        using var context = new MyContext(producerMoq.Object);
        context.TraceIdentifier = traceId;
        var conn = context.Database.GetDbConnection();
        conn.Open();
        context.Database.EnsureCreated();

        var blog = new Blog { RequiredProp = "test" };
        blog.AddTestEvent(testEvent);
        context.Blogs.Add(blog);

        // Act
        await context.SaveChangesAsync();

        // Assert
        Assert.Single(context.Blogs);
        Assert.Empty(blog.GetEvents());
        producerMoq.VerifyAll();

        Assert.Equal(context.TraceIdentifier, testEvent.TraceIdentifier);
    }

    [Fact]
    public async Task SaveChanges_dispatches_domain_events_when_save_is_successful()
    {
        // Arrange
        var testEvent = new TestEvent();
        var producerMoq = new Mock<IProducer>(MockBehavior.Strict);
        producerMoq
            .Setup(p => p.PublishAsync(testEvent, It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        using var context = new MyContext(producerMoq.Object);
        var conn = context.Database.GetDbConnection();
        conn.Open();
        context.Database.EnsureCreated();

        var blog = new Blog { RequiredProp = "test" };
        blog.AddTestEvent(testEvent);
        context.Blogs.Add(blog);

        // Act
        await context.SaveChangesAsync();

        // Assert
        Assert.Single(context.Blogs);
        Assert.Empty(blog.GetEvents());
        producerMoq.VerifyAll();
    }

    [Fact]
    public async Task SaveChanges_does_not_dispatch_domain_events_when_explicitly_requested()
    {
        // Arrange
        var testEvent = new TestEvent();
        var producerMoq = new Mock<IProducer>(MockBehavior.Strict);

        using var context = new MyContext(producerMoq.Object);
        var conn = context.Database.GetDbConnection();
        conn.Open();
        context.Database.EnsureCreated();

        var blog = new Blog { RequiredProp = "test" };
        blog.AddTestEvent(testEvent);
        context.Blogs.Add(blog);

        // Act
        await context.SaveChangesAsync(deferEventPublishing: true, 0);

        // Assert
        Assert.Single(context.Blogs);
        Assert.Empty(blog.GetEvents());
        producerMoq.Verify(
            p => p.PublishAsync(It.IsAny<TestEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task PublishDeferredEvents_dispatch_deferred_domain_events()
    {
        // Arrange
        var testEvent = new TestEvent();
        var producerMoq = new Mock<IProducer>(MockBehavior.Strict);
        producerMoq
            .Setup(p => p.PublishAsync(testEvent, It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        using var context = new MyContext(producerMoq.Object);
        var conn = context.Database.GetDbConnection();
        conn.Open();
        context.Database.EnsureCreated();

        var blog = new Blog { RequiredProp = "test" };
        blog.AddTestEvent(testEvent);
        context.Blogs.Add(blog);
        await context.SaveChangesAsync(deferEventPublishing: true, 0);

        // Act
        await context.PublishDeferredEvents();

        // Assert
        Assert.Single(context.Blogs);
        Assert.Empty(blog.GetEvents());
        producerMoq.VerifyAll();
    }

    private class MyContext : TestContext
    {
        public DbSet<Blog> Blogs { get; set; }
        public MyContext() : base(new DbContextOptionsBuilder().UseSqlite("DataSource=:memory:").Options) { }
        public MyContext(IProducer producer) : base(new DbContextOptionsBuilder().UseSqlite("DataSource=:memory:").Options) => Producer = producer;
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Blog>().Property(p => p.RequiredProp).IsRequired();
        }
    }

    private class Blog : Entity
    {
        public DateTime DateCreated { get; set; }
        public string RequiredProp { get; set; }
        public void AddTestEvent(TestEvent testEvent = null) => AddEvent(testEvent ?? new TestEvent());
    }

    private class TestEvent : DomainEvent
    {
    }
}
