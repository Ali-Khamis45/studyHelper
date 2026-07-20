namespace AiStudyOS.Application.Common.Options;

public class PlannerOptions
{
    public const string SectionName = "Planner";

    public int RecentHistoryDays { get; init; } = 7;
    public int WeekViewDays { get; init; } = 7;
}
