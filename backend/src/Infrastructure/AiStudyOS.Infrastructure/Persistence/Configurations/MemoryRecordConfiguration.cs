using AiStudyOS.Domain.Mentor;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiStudyOS.Infrastructure.Persistence.Configurations;

public class MemoryRecordConfiguration : IEntityTypeConfiguration<MemoryRecord>
{
    public void Configure(EntityTypeBuilder<MemoryRecord> builder)
    {
        builder.ToTable("mentor_memory_records");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Type).IsRequired().HasConversion<string>().HasMaxLength(32);
        builder.Property(m => m.Topic).HasMaxLength(200);
        builder.Property(m => m.Content).IsRequired();
        builder.Property(m => m.SourceType).IsRequired().HasMaxLength(32);

        builder.HasIndex(m => new { m.UserId, m.Type, m.Salience });
    }
}
