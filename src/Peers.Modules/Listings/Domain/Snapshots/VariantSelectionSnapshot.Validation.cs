using Peers.Core.Domain.Errors;
using Peers.Modules.Listings.Domain.Validation;

namespace Peers.Modules.Listings.Domain.Snapshots;

public partial record VariantSelectionSnapshot
{
    internal void Validate(ValidationContext ctx)
    {
        // Must contain exactly one ref per axis in schema
        if (Selections.Count != ctx.AxesSnapshot.Axes.Count)
        {
            throw StateError($"Selection refs count '{Selections.Count}' does not match axes count '{ctx.AxesSnapshot.Axes.Count}'.");
        }

        foreach (var sel in Selections)
        {
            if (!ctx.AxisByDefKey.TryGetValue(sel.DefinitionKey, out var axis))
            {
                throw StateError($"Selection ref axis '{sel}' not found.");
            }

            // Choice must exist on axis
            if (!axis.Choices.Any(c => c.Key == sel.ChoiceKey))
            {
                throw StateError($"Selection ref choice '{sel}' not found on axis '{axis}'.");
            }
        }

        InvalidDomainStateException StateError(string message) => throw new InvalidDomainStateException(this, message);
    }
}
