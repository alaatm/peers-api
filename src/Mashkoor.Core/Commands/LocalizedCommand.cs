namespace Mashkoor.Core.Commands;

/// <summary>
/// Represents a localized command.
/// </summary>
public abstract record LocalizedCommand : ICommand
{
    /// <summary>
    /// Returns current thread language.
    /// </summary>
    public string Lang => Localization.Lang.GetCurrent();
}
