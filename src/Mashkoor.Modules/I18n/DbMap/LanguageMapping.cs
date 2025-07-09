using Humanizer;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mashkoor.Modules.I18n.Domain;

namespace Mashkoor.Modules.I18n.DbMap;

internal sealed class LanguageMapping : IEntityTypeConfiguration<Language>
{
    public void Configure(EntityTypeBuilder<Language> builder)
    {
        builder.Property(p => p.Id).HasMaxLength(2);
        builder.Property(p => p.Name).HasMaxLength(64);
        builder.ToTable(nameof(Language).Underscore(), "i18n");
    }
}
