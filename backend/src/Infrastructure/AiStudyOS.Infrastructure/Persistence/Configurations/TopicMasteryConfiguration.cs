using AiStudyOS.Domain.Quiz;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiStudyOS.Infrastructure.Persistence.Configurations;

public class TopicMasteryConfiguration : IEntityTypeConfiguration<TopicMastery>
{
    public void Configure(EntityTypeBuilder<TopicMastery> builder)
    {
        builder.ToTable("topic_mastery");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Topic).IsRequired().HasMaxLength(300);

        builder.HasIndex(m => new { m.UserId, m.Topic }).IsUnique();
    }
}
