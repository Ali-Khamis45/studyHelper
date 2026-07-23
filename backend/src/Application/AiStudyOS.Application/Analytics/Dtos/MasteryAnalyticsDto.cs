using AiStudyOS.Application.Analytics.Dtos.Charts;
using AiStudyOS.Application.Quiz.Dtos;

namespace AiStudyOS.Application.Analytics.Dtos;

public record MasteryAnalyticsDto(
    IReadOnlyList<TopicMasteryDto> WeakTopics,
    IReadOnlyList<TopicMasteryDto> StrongTopics,
    IReadOnlyList<RadarAxisDto> Radar,
    IReadOnlyList<ChartPointDto> EvolutionTrend);
