using AiStudyOS.Application.Common.Exceptions;
using AiStudyOS.Application.Common.Interfaces;
using AiStudyOS.Application.Quiz.Dtos;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace AiStudyOS.Application.Quiz.Queries.GetQuiz;

public class GetQuizQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser) : IQueryHandler<GetQuizQuery, QuizDto>
{
    public async ValueTask<QuizDto> Handle(GetQuizQuery query, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new InvalidCredentialsException();

        var quiz = await db.Quizzes.FirstOrDefaultAsync(q => q.Id == query.QuizId && q.UserId == userId, ct)
            ?? throw new NotFoundException(nameof(Domain.Quiz.Quiz), query.QuizId);

        var questions = await db.QuizQuestions.Where(q => q.QuizId == quiz.Id).OrderBy(q => q.Order).ToListAsync(ct);

        return QuizDto.FromDomain(quiz, questions);
    }
}
