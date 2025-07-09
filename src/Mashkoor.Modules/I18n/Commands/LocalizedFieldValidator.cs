using Mashkoor.Modules.I18n.Domain;

namespace Mashkoor.Modules.I18n.Commands;

public sealed class LocalizedFieldValidator : AbstractValidator<LocalizedField>
{
    public LocalizedFieldValidator([NotNull] IStrLoc l)
    {
        RuleFor(p => p.Language).NotEmpty().WithName($"{l["Name"]}.{l["Language"]}");
        RuleFor(p => p.Value).NotEmpty().WithName($"{l["Name"]}.{l["Value"]}");
    }
}
