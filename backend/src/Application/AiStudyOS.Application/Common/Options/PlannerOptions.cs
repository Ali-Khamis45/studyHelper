namespace AiStudyOS.Application.Common.Options;

public class PlannerOptions
{
    public const string SectionName = "Planner";

    public int RecentHistoryDays { get; init; } = 7;
    public int WeekViewDays { get; init; } = 7;

    /// <summary>A day whose total estimated task minutes exceeds this is flagged IsOverloaded in the week view.</summary>
    public int DailyWorkloadThresholdMinutes { get; init; } = 240;

    /// <summary>How many rows GetRecommendationHistory returns at most.</summary>
    public int RecommendationHistoryLimit { get; init; } = 30;
}
