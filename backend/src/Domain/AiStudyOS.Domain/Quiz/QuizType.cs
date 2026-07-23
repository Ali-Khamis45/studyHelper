namespace AiStudyOS.Domain.Quiz;

public enum QuizType
{
    /// <summary>A fixed set of AI-generated questions on the requested topic/difficulty.</summary>
    Standard,

    /// <summary>Question difficulty and topic mix are biased toward the student's weakest topics (see TopicMastery).</summary>
    Adaptive,

    /// <summary>Regenerated specifically over the student's current weak topics, ignoring any requested topic.</summary>
    Review,
}
