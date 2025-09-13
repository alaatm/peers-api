using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Peers.Core.Localization.Infrastructure;
using Peers.Modules.I18n.Domain;

namespace Peers.Modules.I18n.DbMap;

internal abstract class TranslationBaseMapping<T, TTr> : IEntityTypeConfiguration<TTr>
    where T : class, IEntity
    where TTr : TranslationBase<T, TTr>
{
    public virtual void Configure(EntityTypeBuilder<TTr> builder)
    {
        builder.HasKey(t => new { t.EntityId, t.LangCode });

        builder.Property(t => t.LangCode).HasMaxLength(2).IsUnicode(false);

        builder
            .HasOne<T>()
            .WithMany(nameof(ILocalizable<,>.Translations))
            .HasForeignKey(p => p.EntityId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne<Language>()
            .WithMany()
            .HasForeignKey(p => p.LangCode);

        ConfigureCore(builder);
    }

    protected abstract void ConfigureCore(EntityTypeBuilder<TTr> builder);
}
