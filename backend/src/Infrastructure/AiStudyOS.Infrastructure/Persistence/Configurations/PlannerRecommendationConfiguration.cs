using AiStudyOS.Domain.Planner;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiStudyOS.Infrastructure.Persistence.Configurations;

public class PlannerRecommendationConfiguration : IEntityTypeConfiguration<PlannerRecommendation>
{
    public void Configure(EntityTypeBuilder<PlannerRecommendation> builder)
    {
        builder.ToTable("planner_recommendations");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.SituationAnalysis).IsRequired();
        builder.Property(r => r.GoalAlignment).IsRequired();
        builder.Property(r => r.Evidence).IsRequired();
        builder.Property(r => r.Recommendation).IsRequired();
        builder.Property(r => r.ImmediateNextAction).IsRequired();
        builder.Property(r => r.AgentType).IsRequired().HasConversion<string>().HasMaxLength(32);
        builder.Property(r => r.ModelUsed).IsRequired().HasMaxLength(100);
        builder.Property(r => r.Provider).IsRequired().HasMaxLength(50);
        builder.Property(r => r.PromptVersion).HasMaxLength(20);
        builder.Property(r => r.RecommendationReason).IsRequired();
        builder.Property(r => r.RawResponseJson).IsRequired().HasColumnType("jsonb");

        builder.HasIndex(r => new { r.UserId, r.Date });
    }
}
