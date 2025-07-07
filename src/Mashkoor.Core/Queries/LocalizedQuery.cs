namespace Mashkoor.Core.Queries;

/// <summary>
/// Represents a localized query.
/// </summary>
public abstract record LocalizedQuery() : IQuery
{
    /// <summary>
    /// Returns current thread language.
    /// </summary>
    public string Lang => Localization.Lang.GetCurrent();
}
