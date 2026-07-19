namespace AiStudyOS.Application.AI.Retrieval;

public record DocumentChunk(Guid Id, Guid DocumentId, string Content, int OrderIndex);

public interface IChunkRepository
{
    Task<IReadOnlyList<DocumentChunk>> GetByDocumentAsync(Guid documentId, CancellationToken ct);
}
