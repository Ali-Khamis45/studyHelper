using AiStudyOS.Application.Analytics.Dtos.Charts;

namespace AiStudyOS.Application.Analytics.Dtos;

public record StreakAnalyticsDto(int CurrentStreak, int LongestStreak, IReadOnlyList<HeatmapCellDto> CompletionHeatmap);
