using AiStudyOS.Application.Common.Dtos;
using AiStudyOS.Application.Common.Exceptions;
using AiStudyOS.Application.Common.Interfaces;
using AiStudyOS.Application.Common.Options;
using AiStudyOS.Application.Quiz.Dtos;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AiStudyOS.Application.Quiz.Queries.GetQuizzes;

public class GetQuizzesQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser, IOptions<QuizOptions> options)
    : IQueryHandler<GetQuizzesQuery, PagedResult<QuizSummaryDto>>
{
    public async ValueTask<PagedResult<QuizSummaryDto>> Handle(GetQuizzesQuery query, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new InvalidCredentialsException();
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize > 0 ? query.PageSize : options.Value.DefaultPageSize;

        var quizzesQuery = db.Quizzes.Where(q => q.UserId == userId);
        var totalCount = await quizzesQuery.CountAsync(ct);

        var quizzes = await quizzesQuery
            .OrderByDescending(q => q.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var quizIds = quizzes.Select(q => q.Id).ToList();

        var latestAttempts = await db.QuizAttempts
            .Where(a => quizIds.Contains(a.QuizId) && a.Status == Domain.Quiz.AttemptStatus.Completed)
            .GroupBy(a => a.QuizId)
            .Select(g => g.OrderByDescending(a => a.CompletedAtUtc).First())
            .ToListAsync(ct);

        var latestByQuiz = latestAttempts.ToDictionary(a => a.QuizId);

        var items = quizzes.Select(q =>
        {
            latestByQuiz.TryGetValue(q.Id, out var latest);
            return new QuizSummaryDto(q.Id, q.Title, q.Topic, q.Difficulty.ToString(), q.QuizType.ToString(), q.QuestionCount, q.CreatedAtUtc, latest?.Score, latest?.CompletedAtUtc);
        }).ToList();

        return new PagedResult<QuizSummaryDto>(items, totalCount, page, pageSize);
    }
}
