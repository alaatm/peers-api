using Peers.Core.Domain.Errors;
using E = Peers.Modules.Listings.ListingErrors;

namespace Peers.Modules.Listings.Domain.Logistics;

/// <summary>
/// Represents the logistics profile for a listing variant, detailing its physical and handling requirements.
/// </summary>
/// <param name="Dimensions">The physical dimensions of the item in centimeters (cm).</param>
/// <param name="Weight">The weight of the item in kilograms (kg).</param>
/// <param name="Fragile">True if the item is fragile and requires special handling; otherwise, false.</param>
/// <param name="Hazmat">True if the item is classified as hazardous material (hazmat) and subject to specific shipping regulations; otherwise, false.</param>
/// <param name="TemperatureControl">The temperature control requirements for the item during storage and transit, if any.</param>
public sealed record LogisticsProfile(
    Dimensions Dimensions,
    double Weight,
    bool Fragile,
    bool Hazmat,
    TemperatureControl TemperatureControl)
{
    private LogisticsProfile() : this(default!, default!, default!, default!, default!) { }

    /// <summary>
    /// Calculates the volumetric weight in kilograms based on the specified divisor.
    /// </summary>
    /// <param name="divisor">The value by which the volume in cubic units is divided to determine the volumetric weight.</param>
    public double VolumetricWeight(int divisor)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(divisor, 0, nameof(divisor));
        return Math.Round(Dimensions.VolumeCubic / divisor, 3);
    }

    /// <summary>
    /// Calculates the billable weight of the shipment in kilograms, using either the actual or volumetric weight based
    /// on the specified divisor.
    /// </summary>
    /// <param name="divisor">The divisor used to calculate the volumetric weight in kilograms</param>
    public double BillableWeight(int divisor)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(divisor, 0, nameof(divisor));
        return double.Max(Weight, VolumetricWeight(divisor));
    }

    /// <summary>
    /// Validates the current object's dimensions and weight, ensuring that all required constraints are met.
    /// </summary>
    internal void Validate()
    {
        Dimensions.Validate();

        if (Weight <= 0)
        {
            throw new DomainException(E.Logistics.WeightMustBePositive);
        }
    }
}
