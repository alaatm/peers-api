namespace Mashkoor.Core.Domain.Rules;

/// <summary>
/// Represents a business rule contract
/// </summary>
public interface IBusinessRule
{
    /// <summary>
    /// The error title.
    /// </summary>
    string ErrorTitle { get; }
    /// <summary>
    /// The error code.
    /// </summary>
    string? Code { get; }
    /// <summary>
    /// The error messages associated with this business rule.
    /// </summary>
    IReadOnlyList<string> Errors { get; }
    /// <summary>
    /// Returns true when the business rule is broken.
    /// </summary>
    /// <returns></returns>
    bool IsBroken();
}
