using AiStudyOS.Application.Analytics.Dtos.Charts;

namespace AiStudyOS.Application.Analytics.Dtos;

public record QuizAnalyticsDto(
    int AttemptCount,
    double AverageScore,
    double HighestScore,
    double LowestScore,
    IReadOnlyList<ChartPointDto> ScoreTrend,
    IReadOnlyList<DistributionBucketDto> ScoreDistribution);
