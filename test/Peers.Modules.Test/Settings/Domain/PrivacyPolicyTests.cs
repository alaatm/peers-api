using Peers.Core.Localization;
using Peers.Core.Localization.Infrastructure;
using Peers.Modules.Settings.Domain;

namespace Peers.Modules.Test.Settings.Domain;

public class PrivacyPolicyTests
{
    [Fact]
    public void Create_creates_instance()
    {
        // Arrange
        var title = "title ";
        var arabicTitle = "arabicTitle ";
        var body = "body ";
        var arabicBody = "arabicBody ";
        var effectiveDate = DateTime.UtcNow.ToDateOnly();

        // Act
        var policy = PrivacyPolicy.Create(
            effectiveDate,
            [
                PrivacyPolicyTr.Dto.Create(Lang.EnLangCode, title, body),
                PrivacyPolicyTr.Dto.Create(Lang.ArLangCode, arabicTitle, arabicBody),
            ]);

        // Assert
        Assert.Equal(effectiveDate, policy.EffectiveDate);

        Assert.Equal(2, policy.Translations.Count);

        var en = policy.Tr(Lang.EnLangCode);
        Assert.Equal(title.Trim(), en.Title);
        Assert.Equal(body.Trim(), en.Body);

        var ar = policy.Tr(Lang.ArLangCode);
        Assert.Equal(arabicTitle.Trim(), ar.Title);
        Assert.Equal(arabicBody.Trim(), ar.Body);
    }

    [Fact]
    public void Update_updates_instance()
    {
        // Arrange
        var title = "title";
        var arabicTitle = "arabicTitle";
        var body = "body";
        var arabicBody = "arabicBody";
        var effectiveDate = DateTime.UtcNow.ToDateOnly();

        var policy = PrivacyPolicy.Create(
            DateTime.UtcNow.AddDays(-10).ToDateOnly(),
            [
                PrivacyPolicyTr.Dto.Create(Lang.EnLangCode, title, body),
                PrivacyPolicyTr.Dto.Create(Lang.ArLangCode, arabicTitle, arabicBody),
            ]);

        // Act
        policy.Update(
            effectiveDate,
            [
                PrivacyPolicyTr.Dto.Create(Lang.EnLangCode, $" new {title} ", $" new {body} "),
                PrivacyPolicyTr.Dto.Create(Lang.ArLangCode, $" new {arabicTitle} ", $" new {arabicBody} "),
            ]);

        Assert.Equal(effectiveDate, policy.EffectiveDate);

        Assert.Equal(2, policy.Translations.Count);

        var en = policy.Tr(Lang.EnLangCode);
        Assert.Equal($"new {title}", en.Title);
        Assert.Equal($"new {body}", en.Body);

        var ar = policy.Tr(Lang.ArLangCode);
        Assert.Equal($"new {arabicTitle}", ar.Title);
        Assert.Equal($"new {arabicBody}", ar.Body);
    }
}
