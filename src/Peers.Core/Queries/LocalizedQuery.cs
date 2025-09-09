using System.Text.Json.Serialization;

namespace Peers.Core.Queries;

/// <summary>
/// Represents a localized query.
/// </summary>
public abstract record LocalizedQuery() : IQuery
{
    /// <summary>
    /// Returns current thread language.
    /// </summary>
    [JsonIgnore]
    public string Lang => Localization.Lang.GetCurrent();
}
