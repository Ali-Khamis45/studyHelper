using System.Collections.Concurrent;
using System.Reflection;
using AiStudyOS.Application.AI.Prompts;
using AiStudyOS.Domain.Mentor;

namespace AiStudyOS.Infrastructure.AI.Prompts;

/// <summary>
/// Loads prompts from embedded .md resources (AI/Prompts/{AgentType}/{version}.md), each carrying
/// a small YAML-ish frontmatter block (version, description, variables, expectedJsonSchema,
/// supportedModels) above the template body.
/// </summary>
public class FilePromptLibrary : IPromptLibrary
{
    private readonly Assembly _assembly = typeof(FilePromptLibrary).Assembly;
    private readonly ConcurrentDictionary<string, PromptDefinition> _cache = new();

    public Task<PromptDefinition> GetAsync(AgentType agentType, string? version = null, CancellationToken ct = default)
    {
        version ??= "v1";
        var cacheKey = $"{agentType}:{version}";

        if (_cache.TryGetValue(cacheKey, out var cached))
            return Task.FromResult(cached);

        var resourceSuffix = $"AI.Prompts.{agentType}.{version}.md";
        var resourceName = _assembly.GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith(resourceSuffix, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"No embedded prompt found for agent '{agentType}' version '{version}' (expected a resource ending in '{resourceSuffix}').");

        using var stream = _assembly.GetManifestResourceStream(resourceName)!;
        using var reader = new StreamReader(stream);
        var raw = reader.ReadToEnd();

        var definition = Parse(agentType, version, raw);
        _cache[cacheKey] = definition;
        return Task.FromResult(definition);
    }

    private static PromptDefinition Parse(AgentType agentType, string requestedVersion, string raw)
    {
        var lines = raw.Replace("\r\n", "\n").Split('\n');
        if (lines.Length == 0 || lines[0].Trim() != "---")
            throw new InvalidOperationException("Prompt file is missing its opening '---' frontmatter delimiter.");

        var frontmatter = new Dictionary<string, string>();
        var i = 1;
        for (; i < lines.Length && lines[i].Trim() != "---"; i++)
        {
            var colonIndex = lines[i].IndexOf(':');
            if (colonIndex < 0) continue;

            frontmatter[lines[i][..colonIndex].Trim()] = lines[i][(colonIndex + 1)..].Trim();
        }
        i++; // skip the closing '---'

        var template = string.Join('\n', lines.Skip(i)).Trim();

        var variables = SplitList(frontmatter.GetValueOrDefault("variables"));
        var supportedModels = SplitList(frontmatter.GetValueOrDefault("supportedModels"));

        return new PromptDefinition(
            agentType,
            frontmatter.GetValueOrDefault("version", requestedVersion),
            frontmatter.GetValueOrDefault("description", string.Empty),
            variables,
            frontmatter.GetValueOrDefault("expectedJsonSchema"),
            supportedModels,
            template);
    }

    private static IReadOnlyList<string> SplitList(string? value) =>
        (value ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}
