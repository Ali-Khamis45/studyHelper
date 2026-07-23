using AiStudyOS.Application.Common.Exceptions;
using AiStudyOS.Application.Common.Interfaces;
using AiStudyOS.Application.Common.Options;
using AiStudyOS.Application.Quiz.Dtos;
using AiStudyOS.Application.Quiz.Grading;
using AiStudyOS.Domain.Quiz;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AiStudyOS.Application.Quiz.Commands.SubmitQuiz;

public class SubmitQuizCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser, IDateTimeProvider dateTimeProvider, IOptions<QuizOptions> options)
    : ICommandHandler<SubmitQuizCommand, QuizAttemptResultDto>
{
    public async ValueTask<QuizAttemptResultDto> Handle(SubmitQuizCommand command, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new InvalidCredentialsException();
        var now = dateTimeProvider.UtcNow;

        var quiz = await db.Quizzes.FirstOrDefaultAsync(q => q.Id == command.QuizId && q.UserId == userId, ct)
            ?? throw new NotFoundException(nameof(Domain.Quiz.Quiz), command.QuizId);

        var questions = await db.QuizQuestions.Where(q => q.QuizId == command.QuizId).OrderBy(q => q.Order).ToListAsync(ct);
        var answersByQuestion = command.Answers.ToDictionary(a => a.QuestionId, a => a.Answer);

        var attempt = QuizAttempt.Create(quiz.Id, userId, now);
        db.QuizAttempts.Add(attempt);

        var graded = new List<(QuizQuestion Question, string UserAnswer, bool IsCorrect)>();
        foreach (var question in questions)
        {
            var userAnswer = answersByQuestion.GetValueOrDefault(question.Id, string.Empty);
            var isCorrect = QuizGrader.GradeAnswer(question, userAnswer);
            graded.Add((question, userAnswer, isCorrect));

            db.QuizAnswers.Add(QuizAnswer.Create(attempt.Id, question.Id, userAnswer, isCorrect, now));
        }

        var correctCount = graded.Count(g => g.IsCorrect);
        attempt.Complete(correctCount, questions.Count, now);

        var (masteryDeltas, weakTopicsThisAttempt) = await UpdateTopicMasteryAsync(db, userId, graded, now, ct);

        await db.SaveChangesAsync(ct);

        var overallWeakTopics = await db.TopicMasteries
            .Where(m => m.UserId == userId && m.MasteryScore < options.Value.WeakTopicMasteryThreshold)
            .OrderBy(m => m.MasteryScore)
            .Take(options.Value.WeakTopicsDefaultTake)
            .Select(m => m.Topic)
            .ToListAsync(ct);

        var answerResults = graded
            .Select(g => new AnswerResultDto(g.Question.Id, g.Question.Text, g.Question.Topic, g.UserAnswer, g.Question.CorrectAnswer, g.IsCorrect, g.Question.Explanation))
            .ToList();

        var averageDelta = masteryDeltas.Count == 0 ? 0 : masteryDeltas.Average();
        var confidence = QuizGrader.ComputeConfidence(questions.Count);

        return new QuizAttemptResultDto(
            attempt.Id, quiz.Id, quiz.Title, attempt.Score!.Value, correctCount, questions.Count, attempt.CompletedAtUtc!.Value,
            answerResults, weakTopicsThisAttempt, overallWeakTopics, averageDelta, confidence);
    }

    /// <summary>Groups graded answers by topic, applies the weighted EMA update to each topic's TopicMastery row (creating it on a topic's first attempt), and returns each topic's mastery delta plus the topics that scored below the weak-topic threshold in THIS attempt specifically.</summary>
    private static async Task<(List<double> Deltas, List<string> WeakTopicsThisAttempt)> UpdateTopicMasteryAsync(
        IApplicationDbContext db, Guid userId, List<(QuizQuestion Question, string UserAnswer, bool IsCorrect)> graded, DateTime now, CancellationToken ct)
    {
        var byTopic = graded.GroupBy(g => g.Question.Topic);
        var topics = byTopic.Select(g => g.Key).ToList();

        var existingMastery = await db.TopicMasteries
            .Where(m => m.UserId == userId && topics.Contains(m.Topic))
            .ToDictionaryAsync(m => m.Topic, ct);

        var deltas = new List<double>();
        var weakThisAttempt = new List<(string Topic, double Score)>();

        foreach (var group in byTopic)
        {
            var topicScore = QuizGrader.ComputeTopicScore(group.Select(g => (g.Question, g.IsCorrect)));

            double resultingScore;
            if (existingMastery.TryGetValue(group.Key, out var mastery))
            {
                var before = mastery.MasteryScore;
                mastery.ApplyQuizResult(topicScore, now);
                deltas.Add(mastery.MasteryScore - before);
                resultingScore = mastery.MasteryScore;
            }
            else
            {
                var created = TopicMastery.Create(userId, group.Key, topicScore, now);
                db.TopicMasteries.Add(created);
                deltas.Add(created.MasteryScore);
                resultingScore = created.MasteryScore;
            }

            // Append-only snapshot — TopicMastery itself only ever holds the current score, so this
            // is the only real data Analytics' mastery-evolution-over-time chart can plot.
            db.TopicMasteryHistories.Add(TopicMasteryHistory.Create(userId, group.Key, resultingScore, now));

            if (topicScore < 0.6)
                weakThisAttempt.Add((group.Key, topicScore));
        }

        var weakTopicsThisAttempt = weakThisAttempt.OrderBy(w => w.Score).Select(w => w.Topic).ToList();
        return (deltas, weakTopicsThisAttempt);
    }
}
