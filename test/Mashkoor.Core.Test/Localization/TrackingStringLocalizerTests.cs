using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Mashkoor.Core.Localization;
using Microsoft.AspNetCore.Localization;

namespace Mashkoor.Core.Test.Localization;

public class TrackingStringLocalizerTests
{
    [Fact]
    public void Indexer_tracks_missing_keys_when_not_found()
    {
        // Arrange
        var missingKeyTrackerMoq = new Mock<IMissingKeyTrackerService>();
        var httpContextAccessorMoq = new Mock<IHttpContextAccessor>();
        var stringLocalizerFactoryMoq = GetLocalizerFactoryMoq();
        var trackingStringLocalizer = new TrackingStringLocalizer<TrackingStringLocalizerTests>(
            stringLocalizerFactoryMoq.Object,
            httpContextAccessorMoq.Object,
            missingKeyTrackerMoq.Object);

        // Act
        var localizedString1 = trackingStringLocalizer["key"];
        var localizedString2 = trackingStringLocalizer["key{0}", "x"];

        // Assert
        Assert.True(localizedString1.ResourceNotFound);
        missingKeyTrackerMoq.Verify(m => m.TrackMissingKey("key", null), Times.Once);

        Assert.True(localizedString2.ResourceNotFound);
        missingKeyTrackerMoq.Verify(m => m.TrackMissingKey("key{0}", null), Times.Once);
    }

    [Fact]
    public void Indexer_tracks_missing_keys_when_not_found_and_reports_context_language_when_set()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Features.Set<IRequestCultureFeature>(
            new RequestCultureFeature(
                new RequestCulture(new System.Globalization.CultureInfo("en-US")),
                new CustomRequestCultureProvider()));

        var missingKeyTrackerMoq = new Mock<IMissingKeyTrackerService>();
        var httpContextAccessorMoq = new Mock<IHttpContextAccessor>();
        httpContextAccessorMoq.SetupGet(p => p.HttpContext).Returns(context);
        var stringLocalizerFactoryMoq = GetLocalizerFactoryMoq();
        var trackingStringLocalizer = new TrackingStringLocalizer<TrackingStringLocalizerTests>(
            stringLocalizerFactoryMoq.Object,
            httpContextAccessorMoq.Object,
            missingKeyTrackerMoq.Object);

        // Act
        var localizedString1 = trackingStringLocalizer["key"];

        // Assert
        Assert.True(localizedString1.ResourceNotFound);
        missingKeyTrackerMoq.Verify(m => m.TrackMissingKey("key", "en-US"), Times.Once);
    }

    public static Mock<IStringLocalizerFactory> GetLocalizerFactoryMoq()
    {
        var stringLocalizerFactory = new Mock<IStringLocalizerFactory>();
        stringLocalizerFactory
            .Setup(s => s.Create(It.IsAny<Type>()))
            .Returns((Type t) => new SLMoq());
        return stringLocalizerFactory;
    }

    public class SLMoq : IStringLocalizer
    {
        public LocalizedString this[string name] => new(name, name, true);
        public LocalizedString this[string name, params object[] arguments] => new(name, name, true);
        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
            => throw new NotImplementedException();
    }
    public class SLMoq<T> : SLMoq, IStringLocalizer<T>
    {
    }

    private class CustomRequestCultureProvider : RequestCultureProvider
    {
        public override Task<ProviderCultureResult> DetermineProviderCultureResult(HttpContext httpContext) => NullProviderCultureResult;
    }
}
