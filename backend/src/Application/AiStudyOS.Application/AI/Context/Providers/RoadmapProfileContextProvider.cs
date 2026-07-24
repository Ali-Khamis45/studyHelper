using System.Text;

namespace AiStudyOS.Application.AI.Context.Providers;

/// <summary>
/// Renders the caller-gathered intake profile (career goal, experience, hours/week, learning
/// style, target date, language, resource preference) as a context fragment — same mechanism as
/// QuizRequestContextProvider. Highest priority of any Roadmap fragment: these are the
/// non-negotiable generation requirements the model must build the roadmap around.
/// </summary>
public class RoadmapProfileContextProvider : IContextProvider
{
    public string SectionName => "Learning Profile";

    public Task<ContextFragment> BuildAsync(ContextRequest request, CancellationToken ct)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"- careerGoal: {request.Parameters["careerGoal"]}");
        AppendIfPresent(sb, "currentExperience", request.Parameters);
        AppendIfPresent(sb, "existingSkills", request.Parameters);
        AppendIfPresent(sb, "hoursPerWeek", request.Parameters);
        AppendIfPresent(sb, "learningStyle", request.Parameters);
        AppendIfPresent(sb, "targetCompletionDate", request.Parameters);
        AppendIfPresent(sb, "preferredLanguage", request.Parameters);
        AppendIfPresent(sb, "preferredResources", request.Parameters);

        var content = sb.ToString().TrimEnd();
        return Task.FromResult(new ContextFragment(SectionName, content, EstimatedTokens: content.Length / 4, Priority: 250));
    }

    private static void AppendIfPresent(StringBuilder sb, string key, IReadOnlyDictionary<string, object?> parameters)
    {
        if (parameters.TryGetValue(key, out var value) && value is not null && value is not "" )
            sb.AppendLine($"- {key}: {value}");
    }
}
