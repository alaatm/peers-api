using Humanizer;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Peers.Modules.Catalog.Domain.Attributes;
using Peers.Modules.Catalog.Domain.Translations;
using Peers.Modules.I18n.DbMap;

namespace Peers.Modules.Catalog.DbMap;

internal sealed class AttributeDefinitionTrMapping : TranslationBaseMapping<AttributeDefinition, AttributeDefinitionTr>
{
    protected override void ConfigureCore(EntityTypeBuilder<AttributeDefinitionTr> builder)
        => builder.ToTable(nameof(AttributeDefinitionTr).Underscore(), "i18n");
}
