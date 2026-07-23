using AiStudyOS.Application.Common.Exceptions;
using AiStudyOS.Application.Common.Interfaces;
using AiStudyOS.Application.Quiz.Dtos;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace AiStudyOS.Application.Quiz.Commands.RetryQuiz;

/// <summary>
/// Explicit "start a retry" action rather than the frontend silently re-showing cached questions —
/// re-validates ownership and existence, and gives the quiz-taking UI a clean entry point distinct
/// from GenerateQuiz (no new AI call; the same questions are reused, a fresh QuizAttempt is created
/// by the SubmitQuiz that follows).
/// </summary>
public class RetryQuizCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser) : ICommandHandler<RetryQuizCommand, QuizDto>
{
    public async ValueTask<QuizDto> Handle(RetryQuizCommand command, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new InvalidCredentialsException();

        var quiz = await db.Quizzes.FirstOrDefaultAsync(q => q.Id == command.QuizId && q.UserId == userId, ct)
            ?? throw new NotFoundException(nameof(Domain.Quiz.Quiz), command.QuizId);

        var questions = await db.QuizQuestions.Where(q => q.QuizId == quiz.Id).OrderBy(q => q.Order).ToListAsync(ct);

        return QuizDto.FromDomain(quiz, questions);
    }
}
