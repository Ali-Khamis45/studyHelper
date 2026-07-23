using AiStudyOS.Application.Common.Exceptions;
using AiStudyOS.Application.Common.Interfaces;
using AiStudyOS.Application.Quiz.Dtos;
using AiStudyOS.Application.Quiz.Grading;
using AiStudyOS.Domain.Quiz;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace AiStudyOS.Application.Quiz.Queries.GetAttempt;

public class GetAttemptQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser) : IQueryHandler<GetAttemptQuery, QuizAttemptResultDto>
{
    public async ValueTask<QuizAttemptResultDto> Handle(GetAttemptQuery query, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new InvalidCredentialsException();

        var attempt = await db.QuizAttempts.FirstOrDefaultAsync(a => a.Id == query.AttemptId && a.UserId == userId, ct)
            ?? throw new NotFoundException(nameof(QuizAttempt), query.AttemptId);

        var quiz = await db.Quizzes.FirstAsync(q => q.Id == attempt.QuizId, ct);
        var questions = await db.QuizQuestions.Where(q => q.QuizId == attempt.QuizId).ToDictionaryAsync(q => q.Id, ct);
        var answers = await db.QuizAnswers.Where(a => a.AttemptId == attempt.Id).ToListAsync(ct);

        var answerResults = answers
            .Select(a =>
            {
                var question = questions[a.QuestionId];
                return new AnswerResultDto(question.Id, question.Text, question.Topic, a.UserAnswer, question.CorrectAnswer, a.IsCorrect, question.Explanation);
            })
            .ToList();

        var weakTopicsThisAttempt = answers
            .GroupBy(a => questions[a.QuestionId].Topic)
            .Select(g => (Topic: g.Key, Score: QuizGrader.ComputeTopicScore(g.Select(a => (questions[a.QuestionId], a.IsCorrect)))))
            .Where(t => t.Score < 0.6)
            .OrderBy(t => t.Score)
            .Select(t => t.Topic)
            .ToList();

        var confidence = QuizGrader.ComputeConfidence(attempt.TotalCount);

        return new QuizAttemptResultDto(
            attempt.Id, quiz.Id, quiz.Title, attempt.Score ?? 0, attempt.CorrectCount, attempt.TotalCount,
            attempt.CompletedAtUtc ?? attempt.StartedAtUtc, answerResults, weakTopicsThisAttempt,
            // Recommended topics and mastery delta are meaningful relative to "right now" — for a
            // historical read they'd require reconstructing mastery as it stood at submission time,
            // which isn't stored. Confidence is deterministic from TotalCount, so it's recomputed
            // faithfully; the other two are only ever authoritative in SubmitQuiz's live response.
            RecommendedTopics: weakTopicsThisAttempt,
            EstimatedMasteryDelta: 0,
            confidence);
    }
}
