using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;
using Peers.Core.Domain.Errors;
using E = Peers.Modules.Catalog.CatalogErrors;

namespace Peers.Modules.Catalog.Domain.Attributes;

public enum NumericKind { Int, Decimal }

/// <summary>
/// Represents the base definition for a numeric product attribute.
/// </summary>
public abstract class NumericAttributeDefinition : AttributeDefinition
{
    /// <summary>
    /// The identifier of the group attribute definition this numeric attribute belongs to, if any.
    /// </summary>
    public int? GroupDefinitionId { get; set; }
    /// <summary>
    /// The group attribute definition this numeric attribute belongs to, if any.
    /// </summary>
    /// <remarks>
    /// When non-null, this numeric attribute participates in exactly one group and must not be marked IsVariant.
    /// </remarks>
    public GroupAttributeDefinition? GroupDefinition { get; set; }
    [NotMapped] public abstract NumericKind NumericKind { get; }
    [NotMapped] public abstract string? Unit { get; }
    [NotMapped] public abstract decimal? Min { get; }
    [NotMapped] public abstract decimal? Max { get; }
    [NotMapped] public abstract decimal? Step { get; }

    protected NumericAttributeDefinition() : base() { }

    /// <summary>
    /// Initializes a new instance of the NumericAttributeDefinition class with the specified owner, key, kind,
    /// requirement, variant status, and position.
    /// </summary>
    /// <param name="owner">The product type that owns this attribute definition. Cannot be null.</param>
    /// <param name="key">The unique key identifying the attribute within the product type. Cannot be null or empty.</param>
    /// <param name="kind">The kind of attribute, indicating its data type or usage.</param>
    /// <param name="isRequired">A value indicating whether the attribute is required when creating a product or listing.</param>
    /// <param name="isVariant">Indicates whether this attribute's value creates a unique, sellable variant of a listing.</param>
    /// <param name="position">The position of the attribute; used for stable ordering.</param>
    protected NumericAttributeDefinition(
        ProductType owner,
        string key,
        AttributeKind kind,
        bool isRequired,
        bool isVariant,
        int position) : base(owner, key, kind, isRequired, isVariant, position)
    {
    }

    public virtual void ValidateValue(decimal value)
    {
        if (NumericKind is NumericKind.Int && decimal.Truncate(value) != value)
        {
            throw new DomainException(E.AttrValueMustBeIntegral(Key, value));
        }
        if (Min is { } min && value < min)
        {
            throw new DomainException(E.AttrValueMustBeAtLeast(Key, value, min));
        }
        if (Max is { } max && value > max)
        {
            throw new DomainException(E.AttrValueMustBeAtMost(Key, value, max));
        }
        if (Step is { } step)
        {
            var relativeValue = value - (Min ?? 0);
            if (relativeValue % step != 0)
            {
                throw new DomainException(E.AttrValueNotAlignedToStep(Key, value, step));
            }
        }
    }

    internal override void Validate()
    {
        base.Validate();
        if (GroupDefinition is not null && IsVariant)
        {
            throw new InvalidOperationException("Attribute cannot be a variant axis when part of a group.");
        }
    }
}

/// <summary>
/// Represents a numeric attribute definition with optional unit and value range constraints for a product type.
/// </summary>
/// <typeparam name="T">The numeric value type of the attribute.</typeparam>
public abstract class NumericAttributeDefinition<T> : NumericAttributeDefinition
    where T : struct, INumber<T>
{
    public NumericAttrConfig<T> Config { get; set; }
    public override string? Unit => Config.Unit;
    public override decimal? Min => Config.Min is { } v ? decimal.CreateChecked(v) : null;
    public override decimal? Max => Config.Max is { } v ? decimal.CreateChecked(v) : null;
    public override decimal? Step => Config.Step is { } v ? decimal.CreateChecked(v) : null;

    protected NumericAttributeDefinition() : base() { }

    protected NumericAttributeDefinition(
        ProductType owner,
        string key,
        AttributeKind kind,
        bool isRequired,
        bool isVariant,
        int position,
        string? unit,
        T? minValue,
        T? maxValue,
        T? step) : base(owner, key, kind, isRequired, isVariant, position)
    {
        ValidateConfig(minValue, maxValue, step);
        Config = new(minValue, maxValue, step, unit?.Trim().ToLowerInvariant());
    }

    public void ValidateValue(T value) => base.ValidateValue(decimal.CreateChecked(value));

    internal override void Validate()
    {
        base.Validate();
        ValidateConfig(Config.Min, Config.Max, Config.Step);
    }

    private static void ValidateConfig(T? min, T? max, T? step)
    {
        if (min > max)
        {
            throw new InvalidOperationException("Min cannot be greater than Max.");
        }
        if (step is not null && step <= T.Zero)
        {
            throw new InvalidOperationException("Step must be positive.");
        }
    }
}

/// <summary>
/// Represents the definition of an integer attribute for a product type.
/// </summary>
public sealed class IntAttributeDefinition : NumericAttributeDefinition<int>
{
    public override NumericKind NumericKind => NumericKind.Int;

    private IntAttributeDefinition() : base() { }

    internal IntAttributeDefinition(
        ProductType owner,
        string key,
        bool isRequired,
        bool isVariant,
        int position,
        string? unit,
        int? minValue,
        int? maxValue,
        int? step) : base(owner, key, AttributeKind.Int, isRequired, isVariant, position, unit, minValue, maxValue, step)
    {
    }
}

/// <summary>
/// Represents the definition of a decimal numeric attribute for a product type.
/// </summary>
public sealed class DecimalAttributeDefinition : NumericAttributeDefinition<decimal>
{
    public override NumericKind NumericKind => NumericKind.Decimal;

    private DecimalAttributeDefinition() : base() { }

    internal DecimalAttributeDefinition(
        ProductType owner,
        string key,
        bool isRequired,
        bool isVariant,
        int position,
        string? unit,
        decimal? minValue,
        decimal? maxValue,
        decimal? step) : base(owner, key, AttributeKind.Decimal, isRequired, isVariant, position, unit, minValue, maxValue, step)
    {
    }
}
