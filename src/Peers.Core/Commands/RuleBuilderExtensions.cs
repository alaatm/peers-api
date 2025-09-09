using FluentValidation;
using System.Diagnostics.CodeAnalysis;

namespace Peers.Core.Commands;

public static class IRuleBuilderExtensions
{
    /// <summary>
    /// Defines a phone number validator on the current rule builder for string properties.
    /// Validation will fail if the value returned by the lambda is not a valid phone number.
    /// </summary>
    /// <typeparam name="T">Type of object being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder on which the validator should be defined.</param>
    /// <param name="l">The localization string.</param>
    /// <returns></returns>
    public static IRuleBuilderOptions<T, string> PhoneNumber<T>(
        [NotNull] this IRuleBuilder<T, string> ruleBuilder,
        [NotNull] IStrLoc l)
        => ruleBuilder.SetValidator(new PhoneNumberValidator<T>(l));

    /// <summary>
    /// Defines a UAE IBAN validator on the current rule builder for string properties.
    /// Validation will fail if the value returned by the lambda is not a valid UAE IBAN.
    /// </summary>
    /// <typeparam name="T">Type of object being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder on which the validator should be defined.</param>
    /// <param name="l">Localization object for error messages.</param>
    /// <returns></returns>
    public static IRuleBuilderOptions<T, string> IsIban<T>(
        [NotNull] this IRuleBuilder<T, string> ruleBuilder,
        [NotNull] IStrLoc l)
        => ruleBuilder.SetValidator(new IbanValidator<T>(l));
}
