using Microsoft.EntityFrameworkCore;

namespace Mashkoor.Modules.Test.SharedClasses;

public static class InMemoryMashkoorContext
{
    public static MashkoorContext Create(Guid? id = null)
    {
        var options = new DbContextOptionsBuilder().UseInMemoryDatabase(id?.ToString() ?? Guid.NewGuid().ToString());
        var context = new MashkoorContext(options.Options);
        return context;
    }
}
