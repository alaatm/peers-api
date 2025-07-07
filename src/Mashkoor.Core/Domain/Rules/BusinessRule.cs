using Microsoft.Extensions.Localization;

namespace Mashkoor.Core.Domain.Rules;

/// <summary>
/// Represents a business rule
/// </summary>
public abstract class BusinessRule : IBusinessRule
{
    private readonly List<string> _errors = [];

    /// <summary>
    /// The string localizer factory. This should be set upon app startup.
    /// </summary>
    public static IStringLocalizerFactory StringLocalizerFactory { set; get; } = default!;

    /// <summary>
    /// The error title.
    /// </summary>
    public abstract string ErrorTitle { get; }
    /// <summary>
    /// The error code.
    /// </summary>
    public string? Code { get; protected set; } = default!;
    /// <summary>
    /// The error messages associated with this business rule.
    /// </summary>
    public IReadOnlyList<string> Errors => _errors.AsReadOnly();

    /// <summary>
    /// Returns true when the business rule is broken.
    /// </summary>
    /// <returns></returns>
    public abstract bool IsBroken();

    /// <summary>
    /// Appends the specified error message.
    /// </summary>
    /// <param name="message">The error message</param>
    /// <returns></returns>
    protected bool Append(string message)
    {
        _errors.Add(message);
        return true;
    }

    /// <summary>
    /// Checks the specified condition and appends message to error list if condition is true.
    /// </summary>
    /// <param name="condition">The condition to check.</param>
    /// <param name="message">The error message if condition is true.</param>
    /// <returns>The condition.</returns>
    protected bool Check(bool condition, string message)
    {
        if (condition)
        {
            _errors.Add(message);
        }

        return condition;
    }
}
