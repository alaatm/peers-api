using Peers.Core.Domain.Errors;
using Peers.Modules.Catalog.Domain.Attributes;
using E = Peers.Modules.Catalog.CatalogErrors;

namespace Peers.Modules.Catalog.Utils;

internal static class AttributeSchemaUtils
{
    // Throws on cyclic
    public static void EnsureAcyclic(List<AttributeDefinition> attrs)
        => _ = TopoOrderByDependency(attrs);

    // Topological order that keeps parents before children; stable within same depth by Position
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
