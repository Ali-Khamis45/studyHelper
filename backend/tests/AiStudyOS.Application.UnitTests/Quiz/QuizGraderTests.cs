using AiStudyOS.Application.Quiz.Grading;
using AiStudyOS.Domain.Quiz;
using FluentAssertions;

namespace AiStudyOS.Application.UnitTests.Quiz;

public class QuizGraderTests
{
    private static QuizQuestion MultipleChoice(string correctAnswer, Difficulty difficulty = Difficulty.Medium) =>
        QuizQuestion.Create(Guid.NewGuid(), 0, QuestionType.MultipleChoice, "Algebra", difficulty, "What is 2+2?", "[\"3\",\"4\",\"5\",\"6\"]", correctAnswer, "Basic arithmetic.");

    private static QuizQuestion TrueFalse(string correctAnswer) =>
        QuizQuestion.Create(Guid.NewGuid(), 0, QuestionType.TrueFalse, "Algebra", Difficulty.Easy, "2+2=4", "[\"True\",\"False\"]", correctAnswer, "Basic arithmetic.");

    private static QuizQuestion FillBlank(string correctAnswer) =>
        QuizQuestion.Create(Guid.NewGuid(), 0, QuestionType.FillBlank, "Algebra", Difficulty.Easy, "2+2=____", null, correctAnswer, "Basic arithmetic.");

    private static QuizQuestion ShortAnswer(string correctAnswer, Difficulty difficulty = Difficulty.Medium) =>
        QuizQuestion.Create(Guid.NewGuid(), 0, QuestionType.ShortAnswer, "Biology", difficulty, "What is mitosis?", null, correctAnswer, "Cell division.");

    [Fact]
    public void GradeAnswer_MultipleChoice_exact_match_is_correct()
    {
        var question = MultipleChoice("4");
        QuizGrader.GradeAnswer(question, "4").Should().BeTrue();
    }

    [Fact]
    public void GradeAnswer_MultipleChoice_is_case_and_whitespace_insensitive()
    {
        var question = MultipleChoice("Paris");
        QuizGrader.GradeAnswer(question, "  paris  ").Should().BeTrue();
    }

    [Fact]
    public void GradeAnswer_MultipleChoice_wrong_option_is_incorrect()
    {
        var question = MultipleChoice("4");
        QuizGrader.GradeAnswer(question, "5").Should().BeFalse();
    }

    [Fact]
    public void GradeAnswer_TrueFalse_matches_exactly()
    {
        var question = TrueFalse("True");
        QuizGrader.GradeAnswer(question, "True").Should().BeTrue();
        QuizGrader.GradeAnswer(question, "False").Should().BeFalse();
    }

    [Fact]
    public void GradeAnswer_FillBlank_matches_exactly()
    {
        var question = FillBlank("4");
        QuizGrader.GradeAnswer(question, "4").Should().BeTrue();
        QuizGrader.GradeAnswer(question, "5").Should().BeFalse();
    }

    [Fact]
    public void GradeAnswer_ShortAnswer_uses_word_overlap_not_exact_match()
    {
        var question = ShortAnswer("cell division through mitosis");

        QuizGrader.GradeAnswer(question, "mitosis is cell division").Should().BeTrue();
    }

    [Fact]
    public void GradeAnswer_ShortAnswer_completely_unrelated_answer_is_incorrect()
    {
        var question = ShortAnswer("cell division through mitosis");

        QuizGrader.GradeAnswer(question, "photosynthesis converts light to energy").Should().BeFalse();
    }

    [Fact]
    public void GradeAnswer_empty_answer_is_always_incorrect()
    {
        var question = MultipleChoice("4");
        QuizGrader.GradeAnswer(question, "").Should().BeFalse();
        QuizGrader.GradeAnswer(question, "   ").Should().BeFalse();
    }

    [Fact]
    public void ComputeTopicScore_all_correct_scores_1()
    {
        var answered = new[] { (MultipleChoice("4"), true), (TrueFalse("True"), true) };
        QuizGrader.ComputeTopicScore(answered).Should().Be(1.0);
    }

    [Fact]
    public void ComputeTopicScore_all_incorrect_scores_0()
    {
        var answered = new[] { (MultipleChoice("4"), false), (TrueFalse("True"), false) };
        QuizGrader.ComputeTopicScore(answered).Should().Be(0.0);
    }

    [Fact]
    public void ComputeTopicScore_weighs_harder_questions_more_heavily()
    {
        // One Easy (weight 1) wrong, one Hard (weight 2) right: 2/(1+2) = 0.667, not a flat 0.5.
        var answered = new[]
        {
            (MultipleChoice("4", Difficulty.Easy), false),
            (MultipleChoice("4", Difficulty.Hard), true),
        };

        QuizGrader.ComputeTopicScore(answered).Should().BeApproximately(0.667, 0.01);
    }

    [Fact]
    public void ComputeTopicScore_empty_set_scores_0()
    {
        QuizGrader.ComputeTopicScore([]).Should().Be(0);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(5, 0.5)]
    [InlineData(10, 1.0)]
    [InlineData(20, 1.0)]
    public void ComputeConfidence_scales_with_question_count_capped_at_1(int totalCount, double expected)
    {
        QuizGrader.ComputeConfidence(totalCount).Should().BeApproximately(expected, 0.001);
    }
}
