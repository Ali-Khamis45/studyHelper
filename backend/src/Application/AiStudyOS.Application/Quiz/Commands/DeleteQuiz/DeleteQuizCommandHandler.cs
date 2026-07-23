using AiStudyOS.Application.Common.Exceptions;
using AiStudyOS.Application.Common.Interfaces;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace AiStudyOS.Application.Quiz.Commands.DeleteQuiz;

public class DeleteQuizCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser) : ICommandHandler<DeleteQuizCommand, bool>
{
    public async ValueTask<bool> Handle(DeleteQuizCommand command, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new InvalidCredentialsException();

        var quiz = await db.Quizzes.FirstOrDefaultAsync(q => q.Id == command.QuizId && q.UserId == userId, ct);
        if (quiz is null) return false;

        // Questions/attempts/answers cascade-delete at the DB level (see QuizQuestionConfiguration /
        // QuizAttemptConfiguration / QuizAnswerConfiguration FKs).
        db.Quizzes.Remove(quiz);
        await db.SaveChangesAsync(ct);

        return true;
    }
}
