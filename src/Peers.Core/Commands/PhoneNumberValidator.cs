using FluentValidation;
using FluentValidation.Validators;

namespace Peers.Core.Commands;

internal sealed class PhoneNumberValidator<T> : PropertyValidator<T, string?>
{
    private readonly IStrLoc _l;

    public override string Name => "PhoneNumberValidator";

    public PhoneNumberValidator(IStrLoc l) => _l = l;

    public override bool IsValid(ValidationContext<T> context, string? value)
        => value is not null && RegexStatic.PhoneNumberRegex().IsMatch(value);

    protected override string GetDefaultMessageTemplate(string errorCode)
        => "'{PropertyName}' " + _l["must be a valid phone number."];
}
