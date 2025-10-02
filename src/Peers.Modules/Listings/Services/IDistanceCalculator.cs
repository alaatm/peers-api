using NetTopologySuite.Geometries;

namespace Peers.Modules.Listings.Services;

/// <summary>
/// Defines a contract for asynchronously measuring the distance between two points.
/// </summary>
public interface IDistanceCalculator
{
    /// <summary>
    /// Asynchronously calculates the distance between two points.
    /// </summary>
    /// <param name="from">The starting point for the measurement.</param>
    /// <param name="to">The ending point for the measurement.</param>
    /// <param name="ctk">A cancellation token that can be used to cancel the asynchronous operation.</param>
    Task<double> MeasureAsync(Point from, Point to, CancellationToken ctk);
}
