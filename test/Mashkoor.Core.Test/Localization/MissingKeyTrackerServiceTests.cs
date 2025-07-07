using Mashkoor.Core.Localization;

namespace Mashkoor.Core.Test.Localization;

public class MissingKeyTrackerServiceTests
{
    [Fact]
    public void TrackMissingKey_tracks_missing_key()
    {
        // Arrange
        var missingKeyTrackerService = new MissingKeyTrackerService();

        // Act
        missingKeyTrackerService.TrackMissingKey("key", "en");
        missingKeyTrackerService.TrackMissingKey("key1", "xx");
        missingKeyTrackerService.TrackMissingKey("key2", "xx");
        missingKeyTrackerService.TrackMissingKey("key", null);
    }
}
