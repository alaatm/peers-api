using System.Globalization;
using System.Text.Json.Serialization;
using Peers.Core.Cqrs.Pipeline;
using Peers.Core.Domain.Errors;
using Peers.Modules.Catalog.Domain.Attributes;
using E = Peers.Modules.Listings.ListingErrors;

namespace Peers.Modules.Listings.Commands;

public static class SetAttributes
{
    /// <summary>
    /// Represents a request to set attributes and variant axes for a specific product listing.
    /// </summary>
    /// <remarks>
    /// Each entry in the attributes dictionary must use a key that matches a defined attribute or
    /// group in the product type. The value for each key must be a valid discriminated input shape, as described in
    /// <see cref="AttributeInputDto"/>. Mixing scalar and axis shapes for the same key is not allowed. All values must
    /// satisfy the constraints and definitions of the product type, such as valid codes, numeric ranges, and group
    /// member order.
    /// </remarks>
    /// <param name="ListingId">The unique identifier of the product listing to update.</param>
    /// <param name="SnapshotId">The snapshot identifier of the listing. This must match the listing's current snapshot ID to ensure consistency.</param>
    /// <param name="Attributes">A dictionary mapping attribute keys to their corresponding input values, specifying the attributes and variant axes to set for the listing.</param>
    [Authorize(Roles = Roles.Seller)]
    public record Command(
        [property: JsonIgnore()] int ListingId,
        string SnapshotId,
        Dictionary<string, Command.AttributeInputDto> Attributes) : ICommand, IValidatable
    {
        /// <summary>
        /// Discriminated input for setting listing attributes and variant axes.
        /// </summary>
        /// <remarks>
        /// Each entry in the request maps an <em>attribute key</em> (or a <em>group key</em>) to exactly one of the shapes below.
        /// <list type="bullet">
        ///   <item><description>Scalar shapes (<see cref="Numeric"/>, <see cref="Bool"/>, <see cref="Date"/>, <see cref="OptionCodeOrScalarString"/>) are for <strong>non-variant</strong> attributes.</description></item>
        ///   <item><description>Axis shapes(<see cref = "NumericAxis" />, <see cref = "OptionCodeAxis" />, <see cref = "GroupAxis" />) are for <strong>variant</strong> attributes (or groups).</description></item>
        ///   <item><description>Mixing scalar and axis shapes for the same key is not allowed.</description></item>
        ///   <item><description>Keys must match definitions in the product type; values must be valid for the definition (e.g., codes must exist; numeric values must satisfy kind and server-side constraints).</description></item>
        /// </list>
        /// </remarks>
        /// <example>
        /// { "condition": "new", "color": ["black","white"], "pack_size": [24,54,72], "size": [[100,300],[150,400]] }
        /// </example>
        public abstract record AttributeInputDto
        {
            private AttributeInputDto() { }

            public record Axis<T>(List<T> Value) : AttributeInputDto
            {
                public void Validate(string key, bool unique, int? minRequired = null, int? exactRequired = null)
                {
                    if (minRequired is { } min && Value.Count < min)
                    {
                        throw new DomainException(E.AxisReqAtLeastMinValues(key, min));
                    }

                    if (exactRequired is { } exact && Value.Count != exact)
                    {
                        throw new DomainException(E.AxisReqExactlyNValues(key, exact));
                    }

                    if (unique && Value.HasDuplicates(out var dup))
                    {
                        throw new DomainException(E.AxisMustNotHaveDuplicateValues(key, dup!.ToString()!));
                    }
                }

                public override int GetHashCode()
                {
                    var hash = new HashCode();

                    foreach (var d in Value)
                    {
                        hash.Add(d);
                    }

                    return hash.ToHashCode();
                }

                public override string ToString() => $"[{string.Join(", ", Value)}]";
            }

            /// <summary>
            /// Scalar numeric value for a non-variant numeric attribute.
            /// </summary>
            /// <remarks>
            /// Send values in the attribute's base unit (server enforces Min/Max/Step). For integer attributes, the value must be integral.
            /// </remarks>
            public sealed record Numeric(decimal Value) : AttributeInputDto
            {
                public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
            }

            /// <summary>
            /// Scalar boolean for a non-variant boolean attribute.
            /// </summary>
            public sealed record Bool(bool Value) : AttributeInputDto
            {
                public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
            }

            /// <summary>
            /// Scalar ISO date for a non-variant date attribute.
            /// </summary>
            /// <remarks>Format as ISO-8601 (e.g., 2025-10-08).</remarks>
            public sealed record Date(DateOnly Value) : AttributeInputDto
            {
                public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
            }

            /// <summary>
            /// Scalar code for a non-variant enum or lookup attribute or a plain string attribute.
            /// </summary>
            /// <remarks>
            /// The string must be the canonical <em>code/key</em> defined for that option/value (not a display label). For lookups, the code must belong to the attribute's lookup type.
            /// For plain string attributes, any non-empty string is allowed.
            /// </remarks>
            public sealed record OptionCodeOrScalarString(string Value) : AttributeInputDto
            {
                public override string ToString() => Value;
            }

            /// <summary>
            /// List of numeric values for a <strong>variant</strong> numeric attribute (stand-alone axis).
            /// </summary>
            /// <remarks>
            /// Provide the finite set the seller offers (non-empty). Values must be in base units, deduplicated, and integral if the attribute is integer. Members of a <see cref="GroupAxis"/> must not be sent here.
            /// </remarks>
            public sealed record NumericAxis(List<decimal> Value) : Axis<decimal>(Value)
            {
                public bool Equals(NumericAxis? other) => other is not null && Value.SequenceEqual(other.Value);
                public override int GetHashCode() => base.GetHashCode();
                public override string ToString() => base.ToString();
            }

            /// <summary>
            /// List of codes for a <strong>variant</strong> enum or lookup attribute (stand-alone axis).
            /// </summary>
            /// <remarks>
            /// Each string is a canonical option/value code. The list must be non-empty and deduplicated. Plain string attributes are not supported as variant axes.
            /// </remarks>
            public sealed record OptionCodeAxis(List<string> Value) : Axis<string>(Value)
            {
                public bool Equals(OptionCodeAxis? other) => other is not null && Value.SequenceEqual(other.Value);
                public override int GetHashCode() => base.GetHashCode();
                public override string ToString() => base.ToString();
            }

            /// <summary>
            /// Matrix of numeric tuples for a <strong>variant</strong> composite <em>group</em> attribute (e.g., Size = Width×Length).
            /// </summary>
            /// <remarks>
            /// Use the <em>group key</em> as the JSON property. Each inner list represents one offered combination and must align with the group's member order (as defined by the product type).  
            /// The matrix must be non-empty; each row length must equal the number of members; values must be valid for the corresponding member (integral if integer, and within Min/Max/Step).
            /// Do not send the group's members separately in the same request.
            /// </remarks>
            public sealed record GroupAxis(List<NumericAxis> Value) : AttributeInputDto
            {
                public void Validate(string key)
                {
                    if (Value.Count == 0)
                    {
                        throw new DomainException(E.AxisReqAtLeastMinValues(key, 1));
                    }

                    if (Value.HasDuplicates(out var dup))
                    {
                        throw new DomainException(E.AxisMustNotHaveDuplicateValues(key, dup!.ToString()!));
                    }
                }

                public override string ToString() => $"[ {string.Join(", ", Value)} ]";
            }
        }
    }

    /// <summary>
    /// Represents the result of the set attributes operation.
    /// </summary>
    /// <param name="SnapshotId">
    /// The new unique identifier of the listing snapshot.
    /// This value must be passed for further updates on the listing.
    /// </param>
    public sealed record Response(string SnapshotId);

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator([NotNull] IStrLoc l)
        {
            RuleFor(p => p.ListingId).GreaterThan(0);
            RuleFor(p => p.SnapshotId).NotEmpty();
            RuleFor(p => p.Attributes).NotEmpty();
        }
    }

    public sealed class Handler : ICommandHandler<Command>
    {
        private readonly PeersContext _context;
        private readonly TimeProvider _timeProvider;

        public Handler(
            PeersContext context,
            TimeProvider timeProvider)
        {
            _context = context;
            _timeProvider = timeProvider;
        }

        public async Task<IResult> Handle([NotNull] Command cmd, CancellationToken ctk)
        {
            if (await _context.Listings
                .AsSplitQuery()
                .Include(p => p.Variants)
                .Include(p => p.Attributes)
                .Include(p => p.ProductType).ThenInclude(p => p.Index)
                .Include(p => p.ProductType).ThenInclude(p => p.Attributes).ThenInclude(p => ((EnumAttributeDefinition)p).Options)
                .Include(p => p.ProductType).ThenInclude(p => p.Attributes).ThenInclude(p => ((LookupAttributeDefinition)p).AllowedOptions)
                .Include(p => p.ProductType).ThenInclude(p => p.Attributes).ThenInclude(p => ((LookupAttributeDefinition)p).LookupType).ThenInclude(p => p.Options)
                .Include(p => p.ProductType).ThenInclude(p => p.Attributes).ThenInclude(p => ((LookupAttributeDefinition)p).LookupType).ThenInclude(p => p.ParentLinks)
                .Include(p => p.ProductType).ThenInclude(p => p.Attributes).ThenInclude(p => ((LookupAttributeDefinition)p).LookupType).ThenInclude(p => p.ChildLinks)
                .FirstOrDefaultAsync(p => p.Id == cmd.ListingId, ctk) is not { } listing)
            {
                return Result.BadRequest(detail: "Invalid listing.");
            }

            listing.SetAttributes(cmd.SnapshotId, cmd.Attributes, 100, 100, _timeProvider.UtcNow());
            await _context.SaveChangesAsync(ctk);
            return Result.Ok(new Response(listing.Snapshot.SnapshotId));
        }
    }
}
