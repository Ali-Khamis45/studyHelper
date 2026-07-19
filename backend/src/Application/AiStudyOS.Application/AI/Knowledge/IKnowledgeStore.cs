namespace AiStudyOS.Application.AI.Knowledge;

// Interface-only in Phase 1 — no Infrastructure implementation or DI registration yet.
// Prepares the seam for a future Knowledge Graph module.

public record KnowledgeNode(string ConceptId, string Name, string? Description);

public interface IKnowledgeStore
{
    Task<KnowledgeNode?> GetAsync(string conceptId, CancellationToken ct);
    Task UpsertAsync(KnowledgeNode node, CancellationToken ct);
}
