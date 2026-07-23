namespace AiStudyOS.Application.Common.Options;

public class AnalyticsOptions
{
    public const string SectionName = "Analytics";

    public double StrongTopicMasteryThreshold { get; init; } = 0.8;
    public int HeatmapWindowDays { get; init; } = 90;
    public int RadarMaxAxes { get; init; } = 8;
    public int TimelineTake { get; init; } = 20;
    public int WeeklyWindowDays { get; init; } = 7;
    public int MonthlyWindowDays { get; init; } = 30;
    public int WeakStrongTopicsTake { get; init; } = 5;
}
