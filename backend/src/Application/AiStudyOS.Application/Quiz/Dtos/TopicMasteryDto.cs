using AiStudyOS.Domain.Quiz;

namespace AiStudyOS.Application.Quiz.Dtos;

public record TopicMasteryDto(string Topic, double MasteryScore, int AttemptsCount, DateTime LastUpdatedUtc)
{
    public static TopicMasteryDto FromDomain(TopicMastery mastery) => new(mastery.Topic, mastery.MasteryScore, mastery.AttemptsCount, mastery.LastUpdatedUtc);
}
