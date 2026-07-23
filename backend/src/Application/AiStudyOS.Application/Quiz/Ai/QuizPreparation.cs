using AiStudyOS.Application.AI.Agents;
using AiStudyOS.Application.AI.Context;
using AiStudyOS.Application.AI.Prompts;
using AiStudyOS.Domain.Mentor;
using AiStudyOS.Domain.Quiz;

namespace AiStudyOS.Application.Quiz.Ai;

/// <summary>Shared by GenerateQuizCommandHandler (ExecuteAsync path) and QuizGenerationStreamer (ExecuteStreamAsync path), mirroring RecommendationPreparation/MentorOrchestrator.</summary>
public static class QuizPreparation
{
    public static async Task<(AgentDefinition Agent, AiContext Context, PromptDefinition Prompt)> PrepareAsync(
        IAgentRegistry agentRegistry,
        IContextBuilder contextBuilder,
        IPromptLibrary promptLibrary,
        Guid userId,
        string topic,
        Difficulty difficulty,
        int questionCount,
        IReadOnlyList<QuestionType> questionTypes,
        QuizType quizType,
        CancellationToken ct)
    {
        var agentDefinition = agentRegistry.Resolve(AgentType.Quiz);

        var context = await contextBuilder.BuildAsync(
            new ContextRequest(userId, new Dictionary<string, object?>
            {
                ["topic"] = topic,
                ["difficulty"] = difficulty.ToString(),
                ["questionCount"] = questionCount,
                ["questionTypes"] = questionTypes.Select(t => t.ToString()).ToList(),
                ["quizType"] = quizType.ToString(),
            }),
            agentDefinition.ContextProviders,
            ct);

        var prompt = await promptLibrary.GetAsync(AgentType.Quiz, agentDefinition.SystemPrompt.Version, ct);

        return (agentDefinition, context, prompt);
    }
}
