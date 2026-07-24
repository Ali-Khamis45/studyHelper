using Mediator;

namespace AiStudyOS.Application.Roadmap.Commands.DeleteRoadmap;

public record DeleteRoadmapCommand(Guid RoadmapId) : ICommand<bool>;
