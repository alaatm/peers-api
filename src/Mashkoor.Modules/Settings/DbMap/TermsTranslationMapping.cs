using Humanizer;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mashkoor.Modules.I18n.Domain;
using Mashkoor.Modules.Settings.Domain;

namespace Mashkoor.Modules.Settings.DbMap;

internal sealed class TermsTranslationMapping : IEntityTypeConfiguration<TermsTranslation>
{
    public void Configure(EntityTypeBuilder<TermsTranslation> builder)
    {
        builder.HasKey(p => new { p.EntityId, p.LanguageId });
        builder.Property(p => p.LanguageId).HasMaxLength(2);
        builder.Property(p => p.Description).Metadata.RemoveAnnotation("MaxLength");

        builder
            .HasOne(p => p.Entity)
            .WithMany(p => p.Translations)
            .HasForeignKey(p => p.EntityId)
            .IsRequired();

        builder
            .HasOne<Language>()
            .WithMany()
            .HasForeignKey(p => p.LanguageId)
            .IsRequired();

        builder.ToTable(nameof(TermsTranslation).Underscore(), "i18n");
    }
}
