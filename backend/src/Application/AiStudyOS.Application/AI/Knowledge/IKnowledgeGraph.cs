namespace AiStudyOS.Application.AI.Knowledge;

public record KnowledgeRelation(string FromConceptId, string ToConceptId, string RelationType);

public interface IKnowledgeGraph
{
    Task<IReadOnlyList<KnowledgeRelation>> GetRelationsAsync(string conceptId, CancellationToken ct);
    Task<IReadOnlyList<string>> GetPrerequisitesAsync(string conceptId, CancellationToken ct);
}
