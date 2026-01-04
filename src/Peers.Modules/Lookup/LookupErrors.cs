using Peers.Core.Domain.Errors;
using static Peers.Modules.Catalog.CatalogErrors;

namespace Peers.Modules.Lookup;

public static class LookupErrors
{
    /// <summary>
    /// Parent option '{0}' not found in lookup type '{1}'.
    /// </summary>
    public static DomainError ParentOptNotFound(string code, string key) => new(Titles.NotFound, "lookup.opt-not-found", code, key);
    /// <summary>
    /// Child option '{0}' not found in lookup type '{1}'.
    /// </summary>
    public static DomainError ChildOptNotFound(string code, string key) => new(Titles.NotFound, "lookup.opt-not-found", code, key);
    /// <summary>
    /// Key '{0}' must be in lower_snake format and has a "g_" prefix.
    /// </summary>
    public static DomainError KeyFormatInvalid(string key) => new(Titles.ValidationFailed, "common.key-format-invalid", key);
}
