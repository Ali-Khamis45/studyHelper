namespace AiStudyOS.Application.AI.Jobs;

public enum AiJobState { Queued, Running, Succeeded, Failed, Cancelled }

public interface IAiJob
{
    Guid JobId { get; }
    Guid UserId { get; }
    DateTime CreatedAtUtc { get; }
}

public record AiJobStatus(Guid JobId, AiJobState State, int ProgressPercent, string? ResultRef, string? ErrorMessage);

public interface IAiJobQueue
{
    Task<Guid> EnqueueAsync<TJob>(TJob job, CancellationToken ct) where TJob : IAiJob;
    Task<AiJobStatus> GetStatusAsync(Guid jobId, CancellationToken ct);
}
