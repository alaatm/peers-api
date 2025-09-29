using Microsoft.AspNetCore.Http.HttpResults;
using Peers.Core.Queries;
using Peers.Modules.I18n.Domain;
using Peers.Modules.SystemInfo.Queries;

namespace Peers.Modules.Test.SystemInfo.Queries;

[Collection(nameof(IntegrationTestBaseCollection))]
public class ListSupportedLanguagesTests : IntegrationTestBase
{
    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Requires_auth()
        => await AssertCommandAccess(new ListSupportedLanguages.Query());

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Returns_supported_languages()
    {
        // Arrange
        var customer = await EnrollCustomer();

        // Act
        var result = await SendAsync(new ListSupportedLanguages.Query(), customer);

        // Assert
        var okResult = Assert.IsType<Ok<PagedQueryResponse<ListSupportedLanguages.Response>>>(result);
        var data = Assert.IsType<PagedQueryResponse<ListSupportedLanguages.Response>>(okResult.Value);
        Assert.Equal(2, data.Total);
        Assert.Equal(Language.Ar.Name, data.Data[0].Name);
        Assert.Equal(Language.Ar.Id, data.Data[0].Code);
        Assert.Equal(Language.Ar.Dir, data.Data[0].Dir);
        Assert.Equal(Language.En.Name, data.Data[1].Name);
        Assert.Equal(Language.En.Id, data.Data[1].Code);
        Assert.Equal(Language.En.Dir, data.Data[1].Dir);
    }
}

public class ListSupportedLanguagesResponseTests
{
    [Fact]
    public void FromSysLanguages_returns_supported_languages()
    {
        // Arrange & act
        var result = ListSupportedLanguages.Response.FromSysLanguages();

        // Assert
        Assert.Equal(Language.SupportedLanguages.Length, result.Length);
        for (var i = 0; i < Language.SupportedLanguages.Length; i++)
        {
            Assert.Equal(Language.SupportedLanguages[i].Name, result[i].Name);
            Assert.Equal(Language.SupportedLanguages[i].Id, result[i].Code);
            Assert.Equal(Language.SupportedLanguages[i].Dir, result[i].Dir);
        }
    }
}
