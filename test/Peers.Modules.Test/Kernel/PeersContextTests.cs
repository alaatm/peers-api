using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.EntityFrameworkCore;
using Peers.Core.Domain;

namespace Peers.Modules.Test.Kernel;

public sealed class PeersContextTests
{
    private static readonly string _connStr = TestConfig.GetConnectionString("context", "ConnStr");

    [Fact]
    public void DbContext_has_all_AggregateRoot_DbSets_defined()
    {
        var aggregateRoots = from t in typeof(PeersContext).Assembly.GetTypes()
                             where typeof(IAggregateRoot).IsAssignableFrom(t) && !typeof(IAggregateRoot).IsAssignableFrom(t.BaseType)
                             select t;

        var definedDbSets = from e in typeof(PeersContext).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                            let t = e.PropertyType
                            where t.IsGenericType && t.GetGenericTypeDefinition().IsAssignableFrom(typeof(DbSet<>))
                            select t.GetGenericArguments().Single();

        var missingEntities = new List<string>();
        foreach (var entity in aggregateRoots)
        {
            if (definedDbSets.Contains(entity))
            {
                continue;
            }

            missingEntities.Add(entity.Name);
        }

        Assert.True(missingEntities.Count == 0,
            "The following entities are not defined in application DbContext:\r\n" + string.Join(Environment.NewLine, missingEntities));
    }

    [SkippableFact]
    public void Migrates_with_no_exceptions()
    {
        Skip.If(TestConfig.IsCi && RuntimeInformation.IsOSPlatform(OSPlatform.Windows));

        var options = new DbContextOptionsBuilder<PeersContext>().UseSqlServer(_connStr, p => p.UseNetTopologySuite()).Options;
        using var context = new PeersContext(options);
        context.Database.EnsureDeleted();

        // Act and assert
        context.Database.Migrate();
    }
}
