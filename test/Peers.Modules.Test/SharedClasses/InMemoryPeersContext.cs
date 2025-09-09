using Microsoft.EntityFrameworkCore;

namespace Peers.Modules.Test.SharedClasses;

public static class InMemoryPeersContext
{
    public static PeersContext Create(Guid? id = null)
    {
        var options = new DbContextOptionsBuilder().UseInMemoryDatabase(id?.ToString() ?? Guid.NewGuid().ToString());
        var context = new PeersContext(options.Options);
        return context;
    }
}
