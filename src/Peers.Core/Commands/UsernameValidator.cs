using FluentValidation;
using FluentValidation.Validators;

namespace Peers.Core.Commands;

internal sealed class UsernameValidator<T> : PropertyValidator<T, string?>
{
    private readonly IStrLoc _l;

    public override string Name => "UsernameValidator";

    public UsernameValidator(IStrLoc l) => _l = l;

    public override bool IsValid(ValidationContext<T> context, string? value)
        => value is not null && RegexStatic.UsernameRegex().IsMatch(value);

    protected override string GetDefaultMessageTemplate(string errorCode)
        => "'{PropertyName}' " + _l["must start with a letter and can contain only letters, numbers, and underscores, with a minimum length of 4 characters."];
}
