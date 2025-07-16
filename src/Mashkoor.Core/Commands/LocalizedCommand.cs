using System.Text.Json.Serialization;

namespace Mashkoor.Core.Commands;

/// <summary>
/// Represents a localized command.
/// </summary>
public abstract record LocalizedCommand : ICommand
{
    /// <summary>
    /// Returns current thread language.
    /// </summary>
    [JsonIgnore]
    public string Lang => Localization.Lang.GetCurrent();
}
