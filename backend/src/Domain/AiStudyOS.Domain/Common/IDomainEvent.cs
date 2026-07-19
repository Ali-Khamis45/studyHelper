namespace AiStudyOS.Domain.Common;

public interface IDomainEvent
{
    Guid EventId => Guid.NewGuid();
    DateTime OccurredAtUtc => DateTime.UtcNow;
}
