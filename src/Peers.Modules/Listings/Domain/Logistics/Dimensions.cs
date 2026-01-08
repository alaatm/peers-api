using System.Text.Json.Serialization;
using Peers.Core.Domain.Errors;
using E = Peers.Modules.Listings.ListingErrors;

namespace Peers.Modules.Listings.Domain.Logistics;

/// <summary>
/// Represents the physical dimensions of an object, including length, width, and height. Unit is centimeters (cm).
/// </summary>
/// <param name="Length">The length of the object in centimeters.</param>
/// <param name="Width">The width of the object in centimeters.</param>
/// <param name="Height">The height of the object in centimeters.</param>
public sealed record Dimensions(
    double Length,
    double Width,
    double Height)
{
    /// <summary>
    /// The longest side of the object in centimeters.
    /// </summary>
    [JsonIgnore]
    public double LongestSide => double.Max(Length, double.Max(Width, Height));
    /// <summary>
    /// The girth of the object in centimeters.
    /// </summary>
    [JsonIgnore]
    public double Girth => Length + (2 * (Width + Height));
    /// <summary>
    /// The volume of the object in cubic centimeters.
    /// </summary>
    [JsonIgnore]
    public double VolumeCubic => Length * Width * Height;

    /// <summary>
    /// Validates that the dimensions are positive values.
    /// </summary>
    internal void Validate()
    {
        if (Length <= 0 || Width <= 0 || Height <= 0)
        {
            throw new DomainException(E.Logistics.DimensionsMustBePositive);
        }
    }
}
