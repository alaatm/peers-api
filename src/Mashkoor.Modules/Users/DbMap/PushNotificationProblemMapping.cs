using Humanizer;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mashkoor.Modules.Users.Domain;

namespace Mashkoor.Modules.Users.DbMap;

internal sealed class PushNotificationProblemMapping : IEntityTypeConfiguration<PushNotificationProblem>
{
    public void Configure(EntityTypeBuilder<PushNotificationProblem> builder)
    {
        builder.HasIndex(p => p.Token);
        builder.HasIndex(p => p.ErrorCode);

        builder.Property(p => p.Token).HasMaxLength(400);

        builder.ToTable(nameof(PushNotificationProblem).Underscore(), "dbo");
    }
}
