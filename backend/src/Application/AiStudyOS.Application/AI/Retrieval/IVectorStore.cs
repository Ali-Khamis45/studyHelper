namespace AiStudyOS.Application.AI.Retrieval;

// Interface-only in Phase 1 — no pgvector, no adapter, no DI registration.
// Prepares the seam for future RAG (PDF Intelligence, Flashcards, Knowledge phases).

public record VectorRecord(string Id, float[] Embedding, IReadOnlyDictionary<string, string> Metadata);

public record VectorFilter(IReadOnlyDictionary<string, string> MatchExact);

public record VectorMatch(string Id, float Score, IReadOnlyDictionary<string, string> Metadata);

public interface IVectorStore
{
    Task UpsertAsync(VectorRecord record, CancellationToken ct);
    Task<IReadOnlyList<VectorMatch>> QueryAsync(float[] embedding, int topK, VectorFilter? filter, CancellationToken ct);
}
