using Peers.Core.Domain.Errors;
using Peers.Modules.Catalog.Domain.Attributes;
using E = Peers.Modules.Catalog.CatalogErrors;

namespace Peers.Modules.Catalog.Utils;

/// <summary>
/// Provides utility methods for validating and ordering attribute definitions based on their dependencies.
/// </summary>
internal static class AttributeSchemaUtils
{
    /// <summary>
    /// Ensures that the specified collection of attribute definitions does not contain any cyclic dependencies
    /// and throws a <see cref="DomainException"/> if a cycle is detected.
    /// </summary>
    /// <param name="attrs">The list of attribute definitions to validate for acyclic dependencies. Cannot be null.</param>
    public static void EnsureAcyclic(List<AttributeDefinition> attrs)
        => _ = TopoOrderByDependency(attrs);

    /// <summary>
    /// Returns a list of attribute definitions ordered so that each parent attribute appears before its dependent
    /// children, preserving the original order among attributes at the same dependency depth by their position.
    /// </summary>
    /// <remarks>This method performs a stable topological sort based on attribute dependencies. It is useful
    /// for scenarios where attributes must be processed in dependency order, such as validation or serialization. The
    /// method does not modify the input list.</remarks>
    /// <param name="attrs">The list of attribute definitions to order. Each attribute must have a unique key. Attributes that represent
    /// dependencies must reference other attributes present in the list.</param>
    public static List<AttributeDefinition> TopoOrderByDependency(List<AttributeDefinition> attrs)
    {
        var byKey = attrs.ToDictionary(d => d.Key, StringComparer.Ordinal);
        var result = new List<AttributeDefinition>(byKey.Count);
        var visiting = new HashSet<string>(StringComparer.Ordinal);
        var visited = new HashSet<string>(StringComparer.Ordinal);
        var depthMap = new Dictionary<string, int>(StringComparer.Ordinal);

        foreach (var attr in attrs)
        {
            if (!visited.Contains(attr.Key))
            {
                Dfs(attr.Key);
            }
        }

        // Sort by depth (parents first), then by Position, then by key as a tie-breaker
        return [.. result
            .OrderBy(d => depthMap[d.Key])
            .ThenBy(d => d.Position)
            .ThenBy(d => d.Key, StringComparer.Ordinal)];

        int Dfs(string key)
        {
            if (!visiting.Add(key))
            {
                throw new DomainException(E.CyclicDependency);
            }

            var depth = 0;
            var attr = byKey[key];

            if (attr is EnumAttributeDefinition eattr &&
                eattr.DependsOn is { } parentAttr)
            {
                if (!byKey.ContainsKey(parentAttr.Key))
                {
                    throw new DomainException(E.CrossPTAttrDep(attr.Key, parentAttr.Key));
                }

                depth = 1 + (
                    visited.Contains(parentAttr.Key)
                        ? depthMap[parentAttr.Key]
                        : Dfs(parentAttr.Key)
                    );
            }

            visiting.Remove(key);
            visited.Add(key);
            depthMap[key] = depth;
            result.Add(attr);

            return depth;
        }
    }
}
