using AiStudyOS.Domain.Quiz;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiStudyOS.Infrastructure.Persistence.Configurations;

public class TopicMasteryHistoryConfiguration : IEntityTypeConfiguration<TopicMasteryHistory>
{
    public void Configure(EntityTypeBuilder<TopicMasteryHistory> builder)
    {
        builder.ToTable("topic_mastery_history");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.Topic).IsRequired().HasMaxLength(300);

        builder.HasIndex(h => new { h.UserId, h.RecordedAtUtc });
    }
}
