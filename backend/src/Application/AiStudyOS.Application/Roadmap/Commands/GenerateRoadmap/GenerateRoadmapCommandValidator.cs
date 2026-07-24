using FluentValidation;

namespace AiStudyOS.Application.Roadmap.Commands.GenerateRoadmap;

public class GenerateRoadmapCommandValidator : AbstractValidator<GenerateRoadmapCommand>
{
    public GenerateRoadmapCommandValidator()
    {
        RuleFor(x => x.CareerGoal).NotEmpty().MaximumLength(200);
        RuleFor(x => x.CurrentExperience).MaximumLength(500);
        RuleFor(x => x.ExistingSkills).MaximumLength(500);
        RuleFor(x => x.LearningStyle).MaximumLength(200);
        RuleFor(x => x.PreferredLanguage).MaximumLength(100);
        RuleFor(x => x.PreferredResources).MaximumLength(300);
        RuleFor(x => x.HoursPerWeek).InclusiveBetween(1, 168).When(x => x.HoursPerWeek.HasValue);
    }
}
