using Microsoft.AspNetCore.Http.HttpResults;
using Mashkoor.Modules.I18n.Queries;

namespace Mashkoor.Modules.Test.I18n.Queries;

[Collection(nameof(IntegrationTestBaseCollection))]
public class ListLanguagesTests : IntegrationTestBase
{
    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Requires_auth()
        => await AssertCommandAccess(new ListLanguages.Query());

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Returns_languageCode_list()
    {
        // Arrange
        var manager = await InsertManagerAsync();

        // Act
        var result = await SendAsync(new ListLanguages.Query(), manager);

        // Assert
        var okResult = Assert.IsType<Ok<string[]>>(result);
        var response = okResult.Value;
        Assert.Equal(3, response.Length);
        Assert.Equal((string[])["ar", "en", "ru"], response);
    }
}
