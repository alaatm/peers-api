using Microsoft.Extensions.Caching.Memory;
using Peers.Core.GoogleServices.Configuration;
using Peers.Core.GoogleServices.Maps;
using Peers.Core.GoogleServices.Maps.Models;
using Peers.Core.GoogleServices.Maps.Models.Geocoding;
using RichardSzalay.MockHttp;

namespace Peers.Core.Test.GoogleServices.Maps;

public class GoogleMapsServiceTests
{
    private static readonly GoogleConfig _config = new() { ApiKey = "123" };

    [Theory]
    [InlineData("en")]
    [InlineData("ar")]
    public async Task ReverseGeocodeAsync_builds_and_sends_request(string lang)
    {
        // Arrange
        var cache = new MemoryCache(new MemoryCacheOptions());
        var point = new LatLng(50.34, 87.11);
        var httpMoq = new MockHttpMessageHandler(BackendDefinitionBehavior.Always);
        httpMoq
            .Expect(HttpMethod.Get, $"https://maps.googleapis.com/maps/api/geocode/json?latlng=50.34,87.11&result_type=street_address&language={lang}&key=123")
            .Respond("application/json", /*lang=json,strict*/ "{\"status\":\"OK\",\"results\":[]}");
        var httpClient = httpMoq.ToHttpClient();

        var service = new GoogleMapsService(httpClient, _config, cache);

        // Act
        var result = await service.ReverseGeocodeAsync(point, lang);

        // Assert
        httpMoq.VerifyNoOutstandingExpectation();
        Assert.NotNull(result);
        Assert.Empty(result.Results);

        // Assert cache entry was created
        var cacheKey = $"REVGEOCODE:50.34,87.11:{lang}";
        Assert.True(cache.TryGetValue(cacheKey, out var cachedValue));
        Assert.Equal(result, cachedValue);
    }

    [Theory]
    [InlineData("en")]
    [InlineData("ar")]
    public async Task ReverseGeocodeAsync_returns_cached_entry_when_exist(string lang)
    {
        // Arrange
        var cache = new MemoryCache(new MemoryCacheOptions());
        var point = new LatLng(50.34, 87.11);
        var cachedResponse = new GeocodeResponse("OK", null, []);
        var cacheKey = $"REVGEOCODE:50.34,87.11:{lang}";
        cache.Set(cacheKey, cachedResponse);

        var httpMoq = new MockHttpMessageHandler(BackendDefinitionBehavior.Always);
        var httpClient = httpMoq.ToHttpClient();
        var service = new GoogleMapsService(httpClient, _config, cache);

        // Act
        var result = await service.ReverseGeocodeAsync(point, lang);

        // Assert
        httpMoq.VerifyNoOutstandingExpectation();
        Assert.Equal(cachedResponse, result);
    }

    [Fact]
    public async Task GetDistanceAsyncAsync_builds_and_sends_request()
    {
        // Arrange
        var cache = new MemoryCache(new MemoryCacheOptions());
        var point1 = new LatLng(50.34, 87.11);
        var point2 = new LatLng(51.34, 88.11);
        var httpMoq = new MockHttpMessageHandler(BackendDefinitionBehavior.Always);
        httpMoq
            .Expect(HttpMethod.Get, $"https://maps.googleapis.com/maps/api/distancematrix/json?origins={point1.Latitude},{point1.Longitude}&destinations={point2.Latitude},{point2.Longitude}&key=123")
            .Respond("application/json", /*lang=json,strict*/ "{\"status\":\"OK\",\"rows\":[{\"elements\":[{\"distance\":{\"value\":3600}}]}]}");
        var httpClient = httpMoq.ToHttpClient();

        var service = new GoogleMapsService(httpClient, _config, cache);

        // Act
        var result = await service.GetDistanceAsync(point1, point2);

        // Assert
        httpMoq.VerifyNoOutstandingExpectation();
        Assert.Equal(3600, result);

        // Assert cache entry was created
        var cacheKey = $"DIST:{point1.Latitude},{point1.Longitude}:{point2.Latitude},{point2.Longitude}";
        Assert.True(cache.TryGetValue(cacheKey, out var cachedValue));
        Assert.Equal(result, cachedValue);
    }

    [Fact]
    public async Task GetDistanceAsyncAsync_returns_cached_entry_when_exist()
    {
        // Arrange
        var cache = new MemoryCache(new MemoryCacheOptions());
        var point1 = new LatLng(50.34, 87.11);
        var point2 = new LatLng(51.34, 88.11);
        var cachedDistance = 3600;
        var cacheKey = $"DIST:{point1.Latitude},{point1.Longitude}:{point2.Latitude},{point2.Longitude}";
        cache.Set(cacheKey, cachedDistance);

        var httpMoq = new MockHttpMessageHandler(BackendDefinitionBehavior.Always);
        var httpClient = httpMoq.ToHttpClient();
        var service = new GoogleMapsService(httpClient, _config, cache);

        // Act
        var result = await service.GetDistanceAsync(point1, point2);

        // Assert
        httpMoq.VerifyNoOutstandingExpectation();
        Assert.Equal(cachedDistance, result);
    }

    [Theory]
    [MemberData(nameof(SnapToRoadsAsync_builds_and_sends_request_TestData))]
    public async Task SnapToRoadsAsync_builds_and_sends_request(LatLng[] path, string expectedPathQueryValue)
    {
        // Arrange
        var httpMoq = new MockHttpMessageHandler(BackendDefinitionBehavior.Always);
        httpMoq
            .Expect(HttpMethod.Get, $"https://roads.googleapis.com/v1/snapToRoads?interpolate=true&path={expectedPathQueryValue}&key=123")
            .Respond("application/json", /*lang=json,strict*/ "{\"snappedPoints\":[]}");
        var httpClient = httpMoq.ToHttpClient();

        var service = new GoogleMapsService(httpClient, _config, new MemoryCache(new MemoryCacheOptions()));

        // Act
        var result = await service.SnapToRoadsAsync(path);

        // Assert
        httpMoq.VerifyNoOutstandingExpectation();
        Assert.NotNull(result);
        Assert.Empty(result.SnappedPoints);
    }

    public static TheoryData<LatLng[], string> SnapToRoadsAsync_builds_and_sends_request_TestData() => new()
    {
        {
            new[] { new LatLng (-85.5, 0), new LatLng(-5.5, 0), new LatLng(5.5, 0), new LatLng(85.5, 0) },
            "-85.500000,0.000000|-5.500000,0.000000|5.500000,0.000000|85.500000,0.000000"
        },
        {
            new[] { new LatLng(0, -175.5), new LatLng(0, -85.5), new LatLng(0, -5.5), new LatLng(0, 5.5), new LatLng(0, 85.5), new LatLng(0, 175.5) },
            "0.000000,-175.500000|0.000000,-85.500000|0.000000,-5.500000|0.000000,5.500000|0.000000,85.500000|0.000000,175.500000"
        },
    };
}
