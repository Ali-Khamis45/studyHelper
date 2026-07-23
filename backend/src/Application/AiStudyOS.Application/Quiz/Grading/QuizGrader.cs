using AiStudyOS.Domain.Quiz;

namespace AiStudyOS.Application.Quiz.Grading;

/// <summary>
/// Deterministic, not AI-based: grading a fixed answer key must be fast, free, and reproducible on
/// every submission. MultipleChoice/TrueFalse/FillBlank use exact normalized matching (their answers
/// are short and precise by construction — the prompt requires it, see Prompts/Quiz/v1.md).
/// ShortAnswer uses word-overlap because exact string matching is unreasonably strict for free text
/// ("mitosis is cell division" vs "cell division through mitosis" mean the same thing) — this is a
/// real, honest heuristic, not a claim of semantic/NLP grading.
/// </summary>
public static class QuizGrader
{
    private const double ShortAnswerOverlapThreshold = 0.5;

    public static bool GradeAnswer(QuizQuestion question, string userAnswer)
    {
        var normalizedUser = Normalize(userAnswer);
        var normalizedCorrect = Normalize(question.CorrectAnswer);

        if (normalizedUser.Length == 0) return false;

        return question.Type switch
        {
            QuestionType.MultipleChoice or QuestionType.TrueFalse or QuestionType.FillBlank => normalizedUser == normalizedCorrect,
            QuestionType.ShortAnswer => WordOverlapRatio(normalizedUser, normalizedCorrect) >= ShortAnswerOverlapThreshold,
            _ => throw new ArgumentOutOfRangeException(nameof(question), question.Type, "Unknown question type."),
        };
    }

    /// <summary>Difficulty-weighted correctness for one topic within a single attempt — a Hard question answered correctly counts for more than an Easy one, both here and in the TopicMastery update this score feeds.</summary>
    public static double ComputeTopicScore(IEnumerable<(QuizQuestion Question, bool IsCorrect)> answered)
    {
        var list = answered.ToList();
        if (list.Count == 0) return 0;

        var totalWeight = list.Sum(a => DifficultyWeight(a.Question.Difficulty));
        var earnedWeight = list.Where(a => a.IsCorrect).Sum(a => DifficultyWeight(a.Question.Difficulty));

        return totalWeight == 0 ? 0 : earnedWeight / totalWeight;
    }

    /// <summary>More questions answered gives a statistically sturdier score — a 2-question quiz's percentage is far noisier than a 10-question one.</summary>
    public static double ComputeConfidence(int totalCount) => Math.Min(1.0, totalCount / 10.0);

    private static double DifficultyWeight(Difficulty difficulty) => difficulty switch
    {
        Difficulty.Easy => 1.0,
        Difficulty.Medium => 1.5,
        Difficulty.Hard => 2.0,
        _ => 1.0,
    };

    private static string Normalize(string text) =>
        new string(text.Trim().ToLowerInvariant().Where(c => !char.IsPunctuation(c)).ToArray()).Trim();

    private static double WordOverlapRatio(string a, string b)
    {
        var wordsA = a.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToHashSet();
        var wordsB = b.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToHashSet();
        if (wordsB.Count == 0) return 0;

        var overlap = wordsA.Intersect(wordsB).Count();
        return (double)overlap / wordsB.Count;
    }
}
