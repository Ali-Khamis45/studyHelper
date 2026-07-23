using AiStudyOS.Domain.Mentor;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiStudyOS.Infrastructure.Persistence.Configurations;

public class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.ToTable("mentor_conversations");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Title).IsRequired().HasMaxLength(200);

        builder.HasIndex(c => new { c.UserId, c.IsPinned });
        builder.HasIndex(c => new { c.UserId, c.LastMessageAtUtc });
    }
}
