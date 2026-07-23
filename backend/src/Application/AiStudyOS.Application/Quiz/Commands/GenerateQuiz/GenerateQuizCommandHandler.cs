using AiStudyOS.Application.AI.Agents;
using AiStudyOS.Application.AI.Context;
using AiStudyOS.Application.AI.Kernel;
using AiStudyOS.Application.AI.Prompts;
using AiStudyOS.Application.Common.Exceptions;
using AiStudyOS.Application.Common.Interfaces;
using AiStudyOS.Application.Common.Options;
using AiStudyOS.Application.Quiz.Ai;
using AiStudyOS.Application.Quiz.Dtos;
using AiStudyOS.Domain.Mentor;
using Mediator;
using Microsoft.Extensions.Options;

namespace AiStudyOS.Application.Quiz.Commands.GenerateQuiz;

public class GenerateQuizCommandHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser,
    IDateTimeProvider dateTimeProvider,
    IAgentRegistry agentRegistry,
    IContextBuilder contextBuilder,
    IPromptLibrary promptLibrary,
    IAiKernel aiKernel,
    IOptions<QuizOptions> options) : ICommandHandler<GenerateQuizCommand, QuizDto>
{
    public async ValueTask<QuizDto> Handle(GenerateQuizCommand command, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new InvalidCredentialsException();

        var topic = await QuizTopicResolver.ResolveAsync(db, options, userId, command.Topic, command.QuizType, ct);

        var (_, context, prompt) = await QuizPreparation.PrepareAsync(
            agentRegistry, contextBuilder, promptLibrary, userId, topic, command.Difficulty, command.QuestionCount, command.QuestionTypes, command.QuizType, ct);

        var kernelResult = await aiKernel.ExecuteAsync<QuizGenerationResult>(
            new KernelRequest(AgentType.Quiz, prompt, context, prompt.ExpectedJsonSchema, UserId: userId), ct);

        if (!kernelResult.Success || kernelResult.Data is null)
            throw new AiGenerationFailedException(string.Join("; ", kernelResult.Errors.DefaultIfEmpty("no response")));

        return await QuizFinalizer.FinalizeAsync(db, userId, command.GoalId, command.QuizType, kernelResult.Data, kernelResult.Telemetry, dateTimeProvider.UtcNow, ct);
    }
}
