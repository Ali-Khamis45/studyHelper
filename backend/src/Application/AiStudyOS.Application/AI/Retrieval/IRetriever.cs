namespace AiStudyOS.Application.AI.Retrieval;

public record RetrievalOptions(int TopK = 5);

public record RetrievedChunk(string ChunkId, string Content, float Score);

public interface IRetriever
{
    Task<IReadOnlyList<RetrievedChunk>> RetrieveAsync(string query, RetrievalOptions options, CancellationToken ct);
}
