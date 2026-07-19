namespace AiStudyOS.Application.Integrations;

public interface IIntegrationRegistry
{
    void Register(IExternalIntegration integration);
    IExternalIntegration? Resolve(string key);
    IReadOnlyList<IExternalIntegration> All { get; }
}
