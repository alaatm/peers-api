using System.Collections;

namespace Peers.Modules.Kernel.OpenApi;

/// <summary>
/// Provides methods for generating unique schema identifier strings for .NET types, including support for nested and
/// generic types.
/// </summary>
[ExcludeFromCodeCoverage]
internal static class SchemaIdBuilder
{
    public static string? Build(Type t)
    {
        if (ShouldInline(t))
        {
            return null; // inline, no component
        }

        // Build ParentChild... name for nested types
        var parts = new Stack<string>();
        for (var cur = t; cur is not null; cur = cur.DeclaringType)
        {
            parts.Push(TrimGenericArity(cur.Name));
        }

        var name = string.Concat(parts);

        // Add generic arguments if present, e.g. PagedResultOfItem
        if (t.IsGenericType)
        {
            var argNames = t.GetGenericArguments()
                            .Select(Build); // recurse to handle nested/generics
            name += "Of" + string.Join("And", argNames);
        }

        // To make absolutely collision-proof, include namespace =>
        // var ns = t.Namespace?.Replace(".", "");
        // if (!string.IsNullOrEmpty(ns)) name = ns + name;
        // But this makes names very long, so we skip it for now.

        return name;

        static string TrimGenericArity(string n)
            => (n.IndexOf('`', StringComparison.Ordinal) is var i && i >= 0) ? n[..i] : n;
    }

    private static bool ShouldInline(Type t)
    {
        // unwrap Nullable<T>
        t = Nullable.GetUnderlyingType(t) ?? t;

        // enums: keep as components (helps reuse + shows enum values)
        if (t.IsEnum)
        {
            return false;
        }

        // primitives & common leaf CLR types
        if (t.IsPrimitive)
        {
            return true; // bool, int, double, etc.
        }

        if (t == typeof(string) ||
            t == typeof(decimal) ||
            t == typeof(Guid) ||
            t == typeof(DateTime) ||
            t == typeof(DateTimeOffset) ||
            t == typeof(TimeSpan) ||
            t == typeof(Uri))
        {
            return true;
        }

        // arrays & basic collections/dictionaries → inline
        if (t.IsArray)
        {
            return true;
        }

        if (typeof(IEnumerable).IsAssignableFrom(t) && t != typeof(string))
        {
            return true;
        }

        // catch-all: inline most System.* types that aren't your DTOs
        if (t.Namespace?.StartsWith("System", StringComparison.Ordinal) == true)
        {
            return true;
        }

        // everything else (your records/DTOs) should be components
        return false;
    }
}
