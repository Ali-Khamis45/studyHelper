using AiStudyOS.Domain.Goals;
using AiStudyOS.Domain.Planner;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiStudyOS.Infrastructure.Persistence.Configurations;

public class DailyTaskConfiguration : IEntityTypeConfiguration<DailyTask>
{
    public void Configure(EntityTypeBuilder<DailyTask> builder)
    {
        builder.ToTable("daily_tasks");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Title).IsRequired().HasMaxLength(300);
        builder.Property(t => t.Reasoning).HasMaxLength(1000);
        builder.Property(t => t.Status).IsRequired().HasConversion<string>().HasMaxLength(32);
        builder.Property(t => t.Source).IsRequired().HasConversion<string>().HasMaxLength(32);
        builder.Property(t => t.EnergyLevel).HasConversion<string>().HasMaxLength(16);

        builder.HasIndex(t => new { t.UserId, t.Date });

        // Optional FK: deleting a goal unlinks its tasks (SetNull) rather than deleting task
        // history, matching the original schema's intentionally-nullable WeeklyObjectiveId FK.
        builder.HasOne<Goal>()
            .WithMany()
            .HasForeignKey(t => t.GoalId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
