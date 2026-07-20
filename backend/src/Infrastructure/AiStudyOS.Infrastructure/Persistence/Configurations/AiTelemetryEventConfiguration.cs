using AiStudyOS.Domain.Telemetry;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiStudyOS.Infrastructure.Persistence.Configurations;

public class AiTelemetryEventConfiguration : IEntityTypeConfiguration<AiTelemetryEvent>
{
    public void Configure(EntityTypeBuilder<AiTelemetryEvent> builder)
    {
        builder.ToTable("ai_telemetry_events");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.CorrelationId).IsRequired().HasMaxLength(100);
        builder.Property(e => e.AgentType).IsRequired().HasConversion<string>().HasMaxLength(32);
        builder.Property(e => e.ProviderKey).IsRequired().HasMaxLength(50);
        builder.Property(e => e.Model).IsRequired().HasMaxLength(100);
        builder.Property(e => e.PromptVersion).HasMaxLength(20);
        builder.Property(e => e.ErrorType).HasMaxLength(200);
        builder.Property(e => e.EstimatedCostUsd).HasColumnType("numeric(12,6)");
        builder.Property(e => e.CircuitBreakerState).HasMaxLength(20);
        builder.Property(e => e.CancellationReason).HasMaxLength(200);

        builder.HasIndex(e => new { e.AgentType, e.CreatedAtUtc });
        builder.HasIndex(e => e.CorrelationId);
    }
}
