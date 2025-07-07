using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Mashkoor.Core.Localization;

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
        var localizedString = trackingStringLocalizer["key"];

        // Assert
        Assert.True(localizedString.ResourceNotFound);
        missingKeyTrackerMoq.Verify(m => m.TrackMissingKey("key", null), Times.Once);
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
}
