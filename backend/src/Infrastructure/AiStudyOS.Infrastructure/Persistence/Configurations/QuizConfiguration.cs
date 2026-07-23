using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiStudyOS.Infrastructure.Persistence.Configurations;

public class QuizConfiguration : IEntityTypeConfiguration<Domain.Quiz.Quiz>
{
    public void Configure(EntityTypeBuilder<Domain.Quiz.Quiz> builder)
    {
        builder.ToTable("quizzes");

        builder.HasKey(q => q.Id);

        builder.Property(q => q.Title).IsRequired().HasMaxLength(300);
        builder.Property(q => q.Topic).IsRequired().HasMaxLength(300);
        builder.Property(q => q.Difficulty).IsRequired().HasConversion<string>().HasMaxLength(16);
        builder.Property(q => q.QuizType).IsRequired().HasConversion<string>().HasMaxLength(16);
        builder.Property(q => q.ModelUsed).IsRequired().HasMaxLength(100);
        builder.Property(q => q.PromptVersion).IsRequired().HasMaxLength(20);
        builder.Property(q => q.CorrelationId).IsRequired().HasMaxLength(100);

        builder.HasIndex(q => new { q.UserId, q.CreatedAtUtc });
    }
}
