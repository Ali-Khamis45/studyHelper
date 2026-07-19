namespace AiStudyOS.Application.AI.Retrieval;

public interface IEmbeddingService
{
    Task<float[]> EmbedAsync(string text, CancellationToken ct);
}
