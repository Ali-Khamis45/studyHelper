using AiStudyOS.Application.Common.Dtos;
using AiStudyOS.Application.Common.Exceptions;
using AiStudyOS.Application.Common.Interfaces;
using AiStudyOS.Application.Common.Options;
using AiStudyOS.Application.Quiz.Dtos;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AiStudyOS.Application.Quiz.Queries.GetQuizHistory;

public class GetQuizHistoryQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser, IOptions<QuizOptions> options)
    : IQueryHandler<GetQuizHistoryQuery, PagedResult<QuizHistoryItemDto>>
{
    public async ValueTask<PagedResult<QuizHistoryItemDto>> Handle(GetQuizHistoryQuery query, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new InvalidCredentialsException();
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize > 0 ? query.PageSize : options.Value.DefaultPageSize;

        var attemptsQuery = db.QuizAttempts.Where(a => a.UserId == userId);
        var totalCount = await attemptsQuery.CountAsync(ct);

        var attempts = await attemptsQuery
            .OrderByDescending(a => a.StartedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var quizIds = attempts.Select(a => a.QuizId).Distinct().ToList();
        var quizzesById = await db.Quizzes.Where(q => quizIds.Contains(q.Id)).ToDictionaryAsync(q => q.Id, ct);

        // QuizAttempt has a Cascade FK to Quiz (see QuizAttemptConfiguration), so a quiz that owns
        // any attempt can never be missing from quizzesById here — deleting a quiz deletes its
        // attempts with it.
        var items = attempts
            .Select(a =>
            {
                var quiz = quizzesById[a.QuizId];
                return new QuizHistoryItemDto(a.Id, a.QuizId, quiz.Title, quiz.Topic, a.Score, a.CorrectCount, a.TotalCount, a.Status.ToString(), a.StartedAtUtc, a.CompletedAtUtc);
            })
            .ToList();

        return new PagedResult<QuizHistoryItemDto>(items, totalCount, page, pageSize);
    }
}
