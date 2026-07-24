using AiStudyOS.Application.Roadmap.Dtos;
using Mediator;

namespace AiStudyOS.Application.Roadmap.Commands.GenerateRoadmap;

public record GenerateRoadmapCommand(
    string CareerGoal,
    string? CurrentExperience,
    string? ExistingSkills,
    int? HoursPerWeek,
    string? LearningStyle,
    DateOnly? TargetCompletionDate,
    string? PreferredLanguage,
    string? PreferredResources) : ICommand<RoadmapDto>;
