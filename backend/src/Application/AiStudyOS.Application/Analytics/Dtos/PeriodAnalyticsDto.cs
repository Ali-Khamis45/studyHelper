using AiStudyOS.Application.Analytics.Dtos.Charts;

namespace AiStudyOS.Application.Analytics.Dtos;

/// <summary>Shared shape for /analytics/weekly and /analytics/monthly — same fields, different window.</summary>
public record PeriodAnalyticsDto(StudyTimeStatsDto StudyTime, TaskStatsDto Tasks, IReadOnlyList<ChartPointDto> DailyActivity);
