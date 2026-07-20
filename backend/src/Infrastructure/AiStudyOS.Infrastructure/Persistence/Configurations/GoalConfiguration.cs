using AiStudyOS.Domain.Goals;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiStudyOS.Infrastructure.Persistence.Configurations;

public class GoalConfiguration : IEntityTypeConfiguration<Goal>
{
    public void Configure(EntityTypeBuilder<Goal> builder)
    {
        builder.ToTable("goals");

        builder.HasKey(g => g.Id);

        builder.Property(g => g.Title).IsRequired().HasMaxLength(200);
        builder.Property(g => g.Description).HasMaxLength(2000);
        builder.Property(g => g.Category).IsRequired().HasConversion<string>().HasMaxLength(32);
        builder.Property(g => g.Status).IsRequired().HasConversion<string>().HasMaxLength(32);
        builder.Property(g => g.Priority).IsRequired().HasConversion<string>().HasMaxLength(32);

        builder.HasIndex(g => new { g.UserId, g.Status });
    }
}
