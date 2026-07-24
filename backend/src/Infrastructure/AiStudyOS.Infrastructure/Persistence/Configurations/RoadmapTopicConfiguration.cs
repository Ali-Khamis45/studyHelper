using AiStudyOS.Domain.Roadmap;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiStudyOS.Infrastructure.Persistence.Configurations;

public class RoadmapTopicConfiguration : IEntityTypeConfiguration<RoadmapTopic>
{
    public void Configure(EntityTypeBuilder<RoadmapTopic> builder)
    {
        builder.ToTable("roadmap_topics");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Title).IsRequired().HasMaxLength(300);
        builder.Property(t => t.Description).IsRequired();
        builder.Property(t => t.Difficulty).IsRequired().HasConversion<string>().HasMaxLength(16);
        builder.Property(t => t.PrerequisiteTopicIdsJson).IsRequired().HasColumnType("jsonb").HasDefaultValue("[]");
        builder.Property(t => t.ResourcesJson).IsRequired().HasColumnType("jsonb").HasDefaultValue("[]");
        builder.Property(t => t.SuggestedProjectsJson).IsRequired().HasColumnType("jsonb").HasDefaultValue("[]");
        builder.Property(t => t.LinkedMasteryTopic).IsRequired().HasMaxLength(300);

        builder.HasOne<LearningRoadmap>()
            .WithMany()
            .HasForeignKey(t => t.RoadmapId)
            .OnDelete(DeleteBehavior.Cascade);

        // Self-referencing prerequisite/parent links are id-only (see PrerequisiteTopicIdsJson and
        // ParentTopicId) rather than an EF navigation/FK — a topic can reference any other topic in
        // the same roadmap, not just its tree ancestors, so a single required FK doesn't fit; the
        // Cascade above on RoadmapId already guarantees no orphaned topic once RoadmapId is required.
        builder.HasIndex(t => new { t.RoadmapId, t.ParentTopicId, t.Order });
    }
}
