using AiStudyOS.Application.Common.Dtos;
using AiStudyOS.Application.Quiz.Dtos;
using Mediator;

namespace AiStudyOS.Application.Quiz.Queries.GetQuizHistory;

public record GetQuizHistoryQuery(int Page = 1, int PageSize = 0) : IQuery<PagedResult<QuizHistoryItemDto>>;
