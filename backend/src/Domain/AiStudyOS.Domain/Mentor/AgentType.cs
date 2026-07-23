namespace AiStudyOS.Domain.Mentor;

public enum AgentType
{
    Supervisor,
    Planner,
    Recommendation,
    Tutor,
    Examiner,
    Memory,
    Analytics,
    Focus,

    /// <summary>
    /// Structured quiz generation (single-shot JSON, see QuizGeneratorAgentDefinition) — deliberately
    /// distinct from Examiner, which is Mentor's conversational quiz-practice persona (chat, no
    /// persisted/gradeable questions). AgentRegistry holds exactly one AgentDefinition per AgentType,
    /// so these two genuinely different execution modes need separate slots.
    /// </summary>
    Quiz,

    /// <summary>
    /// Structured AI Insights generation (single-shot JSON — weekly/monthly summary, strengths,
    /// weaknesses, focus areas, risk detection, schedule suggestions). Deliberately distinct from
    /// Analytics, which is Mentor's conversational progress-discussion persona (chat, free text).
    /// </summary>
    Insights,
}
