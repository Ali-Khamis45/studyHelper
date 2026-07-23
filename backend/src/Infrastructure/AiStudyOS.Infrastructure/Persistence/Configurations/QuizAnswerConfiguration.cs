using AiStudyOS.Domain.Quiz;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiStudyOS.Infrastructure.Persistence.Configurations;

public class QuizAnswerConfiguration : IEntityTypeConfiguration<QuizAnswer>
{
    public void Configure(EntityTypeBuilder<QuizAnswer> builder)
    {
        builder.ToTable("quiz_answers");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.UserAnswer).IsRequired();

        builder.HasIndex(a => a.AttemptId);

        builder.HasOne<QuizAttempt>()
            .WithMany()
            .HasForeignKey(a => a.AttemptId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restrict, not Cascade: a second cascade path to quiz_questions (via quizzes -> quiz_answers)
        // alongside the attempt path above is unnecessary — deleting a quiz's attempts already
        // removes every answer before its questions are removed, so this FK's cascade would never
        // actually need to fire, and leaving it Restrict avoids a redundant multi-path cascade.
        builder.HasOne<QuizQuestion>()
            .WithMany()
            .HasForeignKey(a => a.QuestionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
