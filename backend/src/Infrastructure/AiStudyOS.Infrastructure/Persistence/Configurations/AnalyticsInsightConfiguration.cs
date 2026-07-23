using AiStudyOS.Domain.Analytics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiStudyOS.Infrastructure.Persistence.Configurations;

public class AnalyticsInsightConfiguration : IEntityTypeConfiguration<AnalyticsInsight>
{
    public void Configure(EntityTypeBuilder<AnalyticsInsight> builder)
    {
        builder.ToTable("analytics_insights");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.WeeklySummary).IsRequired();
        builder.Property(i => i.MonthlySummary).IsRequired();
        builder.Property(i => i.StrengthsJson).IsRequired();
        builder.Property(i => i.WeaknessesJson).IsRequired();
        builder.Property(i => i.RecommendedFocusAreasJson).IsRequired();
        builder.Property(i => i.SuggestedScheduleImprovementsJson).IsRequired();
        builder.Property(i => i.RiskDetection).IsRequired();
        builder.Property(i => i.ModelUsed).IsRequired().HasMaxLength(100);
        builder.Property(i => i.PromptVersion).HasMaxLength(20);
        builder.Property(i => i.CorrelationId).IsRequired().HasMaxLength(100);

        builder.HasIndex(i => new { i.UserId, i.GeneratedAtUtc });
    }
}
