using System.Numerics;

namespace Peers.Modules.Catalog.Domain.Attributes;

/// <summary>
/// Represents a numeric attribute definition with optional unit and value range constraints for a product type.
/// </summary>
/// <typeparam name="T">The numeric value type of the attribute.</typeparam>
public abstract class NumericAttributeDefinition<T> : AttributeDefinition
    where T : struct, INumber<T>
{
    /// <summary>
    /// The unit of measurement associated with the value, if any.
    /// </summary>
    public string? Unit { get; private set; }
    public NumericAttrConfig<T> Config { get; set; }

    protected NumericAttributeDefinition() : base() { }

    protected NumericAttributeDefinition(
        ProductType owner,
        string key,
        AttributeKind kind,
        bool isRequired,
        int position,
        string? unit,
        T? minValue,
        T? maxValue) : base(owner, key, kind, isRequired, position)
    {
        Unit = unit;
        SetRange(minValue, maxValue);
    }

    private void SetRange(T? min, T? max)
    {
        if (min > max)
        {
            throw new InvalidOperationException("Min cannot be greater than Max.");
        }

        Config = new(min, max);
    }
}

/// <summary>
/// Represents the definition of an integer attribute for a product type.
/// </summary>
public sealed class IntAttributeDefinition : NumericAttributeDefinition<int>
{
    private IntAttributeDefinition() : base() { }

    internal IntAttributeDefinition(
        ProductType owner,
        string key,
        bool isRequired,
        int position,
        string? unit,
        int? minValue,
        int? maxValue) : base(owner, key, AttributeKind.Int, isRequired, position, unit, minValue, maxValue)
    {
    }
}

/// <summary>
/// Represents the definition of a decimal numeric attribute for a product type.
/// </summary>
public sealed class DecimalAttributeDefinition : NumericAttributeDefinition<decimal>
{
    private DecimalAttributeDefinition() : base() { }

    internal DecimalAttributeDefinition(
        ProductType owner,
        string key,
        bool isRequired,
        int position,
        string? unit,
        decimal? minValue,
        decimal? maxValue) : base(owner, key, AttributeKind.Decimal, isRequired, position, unit, minValue, maxValue)
    {
    }
}
