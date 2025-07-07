using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Mashkoor.Core.Identity;

namespace Mashkoor.Core.Test.Identity;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddIdentityInfo_adds_required_services()
    {
        // Act
        var serviceProvider = new ServiceCollection().AddIdentityInfo().BuildServiceProvider();

        // Assert
        serviceProvider.GetRequiredService<IHttpContextAccessor>();
        serviceProvider.GetRequiredService<IIdentityInfo>();
    }
}
