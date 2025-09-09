using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace Peers.Core.Data;

internal sealed class SnakeCaseNameRewriter : IPropertyAddedConvention
{
    public void ProcessPropertyAdded(
        IConventionPropertyBuilder propertyBuilder,
        IConventionContext<IConventionPropertyBuilder> context)
    {
        var property = propertyBuilder.Metadata;
        if (property.DeclaringType is IConventionEntityType convEntityType)
        {
            var ownership = convEntityType!.FindOwnership();
            var navName = ownership?.PrincipalToDependent?.Name;

            // TODO: The following is a temporary hack. We should probably just always set the relational override below,
            // but https://github.com/dotnet/efcore/pull/23834
            var baseColumnName = StoreObjectIdentifier.Create(convEntityType, StoreObjectType.Table) is { } tableIdentifier
                ? property.GetDefaultColumnName(tableIdentifier)
                : property.GetDefaultColumnName();
            var rewrittenName = baseColumnName.Underscore();

            if (navName is not null && rewrittenName.StartsWith($"{navName.Underscore()}_", StringComparison.InvariantCulture))
            {
                rewrittenName = propertyBuilder.Metadata.Name.Underscore();
            }

            propertyBuilder.HasColumnName(rewrittenName);
        }
        else if (property.DeclaringType is IConventionComplexType)
        {
            propertyBuilder.HasColumnName(propertyBuilder.Metadata.Name.Underscore());
        }
    }
}
