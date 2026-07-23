using AiStudyOS.Domain.Mentor;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiStudyOS.Infrastructure.Persistence.Configurations;

public class ConversationMessageConfiguration : IEntityTypeConfiguration<ConversationMessage>
{
    public void Configure(EntityTypeBuilder<ConversationMessage> builder)
    {
        builder.ToTable("mentor_messages");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Role).IsRequired().HasConversion<string>().HasMaxLength(16);
        builder.Property(m => m.Content).IsRequired();
        builder.Property(m => m.AgentType).HasConversion<string>().HasMaxLength(32);
        builder.Property(m => m.ModelUsed).HasMaxLength(100);
        builder.Property(m => m.CorrelationId).HasMaxLength(100);

        builder.HasIndex(m => new { m.ConversationId, m.CreatedAtUtc });

        builder.HasOne<Conversation>()
            .WithMany()
            .HasForeignKey(m => m.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
