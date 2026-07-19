namespace AiStudyOS.Application.AI.Knowledge;

public record Concept(string ConceptId, string Name);

public interface IConceptRepository
{
    Task<Concept?> FindByNameAsync(string name, CancellationToken ct);
}
