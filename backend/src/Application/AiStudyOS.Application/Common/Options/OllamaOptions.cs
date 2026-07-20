namespace AiStudyOS.Application.Common.Options;

public class OllamaOptions
{
    public const string SectionName = "Ollama";

    public string BaseUrl { get; init; } = "http://localhost:11434";
    public string DefaultModel { get; init; } = "llama3.1";
    public int TimeoutSeconds { get; init; } = 120;
}
