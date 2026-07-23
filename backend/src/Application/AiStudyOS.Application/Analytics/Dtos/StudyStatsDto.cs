namespace AiStudyOS.Application.Analytics.Dtos;

/// <summary>Sum of EstimatedMinutes across Completed tasks in each rolling window — the only real, DB-backed proxy for "time studied" this app has (no timer/stopwatch feature exists).</summary>
public record StudyTimeStatsDto(int DailyMinutes, int WeeklyMinutes, int MonthlyMinutes);

public record TaskStatsDto(int Completed, int Skipped, int Rescheduled, int Pending, int Total, double CompletionRatePercent);
