using System.Diagnostics.CodeAnalysis;
using Mashkoor.Core.Domain.Rules;

namespace Mashkoor.Core.Domain;

/// <summary>
/// Represents business rules exception.
/// </summary>
public class BusinessRuleValidationException : Exception
{
    /// <summary>
    /// The list of broken rules.
    /// </summary>
    public IBusinessRule BrokenRule { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="BusinessRuleValidationException"/>.
    /// </summary>
    /// <param name="brokenRule">The broken rule.</param>
    public BusinessRuleValidationException([NotNull] IBusinessRule brokenRule) : base($"{brokenRule.ErrorTitle}:{Environment.NewLine}{string.Join(Environment.NewLine, brokenRule.Errors)}")
        => BrokenRule = brokenRule;

    // Unused
    public BusinessRuleValidationException() => throw new NotImplementedException();
    public BusinessRuleValidationException(string message) : base(message) => throw new NotImplementedException();
    public BusinessRuleValidationException(string message, Exception innerException) : base(message, innerException) => throw new NotImplementedException();
}
