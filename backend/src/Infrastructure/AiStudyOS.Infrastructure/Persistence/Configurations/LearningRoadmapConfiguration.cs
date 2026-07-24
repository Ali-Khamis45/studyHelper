using AiStudyOS.Domain.Roadmap;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiStudyOS.Infrastructure.Persistence.Configurations;

public class LearningRoadmapConfiguration : IEntityTypeConfiguration<LearningRoadmap>
{
    public void Configure(EntityTypeBuilder<LearningRoadmap> builder)
    {
        builder.ToTable("learning_roadmaps");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.CareerGoal).IsRequired().HasMaxLength(200);
        builder.Property(r => r.Title).IsRequired().HasMaxLength(300);
        builder.Property(r => r.Description).IsRequired();
        builder.Property(r => r.Difficulty).IsRequired().HasConversion<string>().HasMaxLength(16);
        builder.Property(r => r.Status).IsRequired().HasConversion<string>().HasMaxLength(16);
        builder.Property(r => r.ModelUsed).IsRequired().HasMaxLength(100);
        builder.Property(r => r.PromptVersion).IsRequired().HasMaxLength(20);
        builder.Property(r => r.CorrelationId).IsRequired().HasMaxLength(100);

        builder.HasIndex(r => new { r.UserId, r.UpdatedAtUtc });
    }
}
