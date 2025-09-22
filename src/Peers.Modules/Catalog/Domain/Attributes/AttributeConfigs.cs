using System.Numerics;
using Peers.Modules.Lookup.Domain;

namespace Peers.Modules.Catalog.Domain.Attributes;

public record struct StringAttrConfig(string? Regex);

public record struct NumericAttrConfig<T>(T? Min, T? Max, string? Unit) where T : struct, INumber<T>;

public record struct LookupAttrConfig(LookupConstraintMode ConstraintMode);
