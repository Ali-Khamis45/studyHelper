using AiStudyOS.Domain.Quiz;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiStudyOS.Infrastructure.Persistence.Configurations;

public class QuizQuestionConfiguration : IEntityTypeConfiguration<QuizQuestion>
{
    public void Configure(EntityTypeBuilder<QuizQuestion> builder)
    {
        builder.ToTable("quiz_questions");

        builder.HasKey(q => q.Id);

        builder.Property(q => q.Type).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(q => q.Topic).IsRequired().HasMaxLength(300);
        builder.Property(q => q.Difficulty).IsRequired().HasConversion<string>().HasMaxLength(16);
        builder.Property(q => q.Text).IsRequired();
        builder.Property(q => q.OptionsJson);
        builder.Property(q => q.CorrectAnswer).IsRequired();
        builder.Property(q => q.Explanation).IsRequired();

        builder.HasIndex(q => new { q.QuizId, q.Order });

        builder.HasOne<Domain.Quiz.Quiz>()
            .WithMany()
            .HasForeignKey(q => q.QuizId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
