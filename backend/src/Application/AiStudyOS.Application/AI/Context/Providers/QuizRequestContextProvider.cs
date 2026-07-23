using System.Text;

namespace AiStudyOS.Application.AI.Context.Providers;

/// <summary>
/// Renders the caller-specified generation parameters (topic, difficulty, question count, allowed
/// question types, quiz mode) as a context fragment — the same mechanism every other instruction
/// reaches the model through (see AiKernel.RenderTemplate, which only ever substitutes one
/// {{context}} placeholder). Highest priority of any Quiz fragment: these are non-negotiable
/// generation requirements, never a candidate for token-budget trimming.
/// </summary>
public class QuizRequestContextProvider : IContextProvider
{
    public string SectionName => "Quiz Request";

    public Task<ContextFragment> BuildAsync(ContextRequest request, CancellationToken ct)
    {
        var topic = (string)request.Parameters["topic"]!;
        var difficulty = (string)request.Parameters["difficulty"]!;
        var questionCount = (int)request.Parameters["questionCount"]!;
        var questionTypes = (IReadOnlyList<string>)request.Parameters["questionTypes"]!;
        var quizType = (string)request.Parameters["quizType"]!;

        var sb = new StringBuilder();
        sb.AppendLine($"- topic: {topic}");
        sb.AppendLine($"- difficulty: {difficulty}");
        sb.AppendLine($"- questionCount: {questionCount}");
        sb.AppendLine($"- allowedQuestionTypes: {string.Join(", ", questionTypes)}");
        sb.AppendLine($"- quizType: {quizType}");

        var content = sb.ToString().TrimEnd();
        return Task.FromResult(new ContextFragment(SectionName, content, EstimatedTokens: content.Length / 4, Priority: 250));
    }
}
