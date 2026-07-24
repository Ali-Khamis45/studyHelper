using Mediator;

namespace AiStudyOS.Application.Roadmap.Commands.CompleteTopic;

public record CompleteTopicCommand(Guid RoadmapId, Guid TopicId, bool Completed) : ICommand<bool>;
