using System.Text.Json;
using AiStudyOS.Application.AI.Telemetry;
using AiStudyOS.Application.Common.Exceptions;
using AiStudyOS.Application.Common.Interfaces;
using AiStudyOS.Application.Roadmap.Dtos;
using AiStudyOS.Domain.Roadmap;

namespace AiStudyOS.Application.Roadmap.Ai;

/// <summary>
/// Turns a parsed RoadmapGenerationResult into a persisted LearningRoadmap + flattened RoadmapTopic
/// tree and returns the resulting RoadmapDto — the one place either the sync or streaming
/// generation path is allowed to persist, mirroring QuizFinalizer/RecommendationFinalizer.
///
/// Sections and (sub)topics all flatten into the same RoadmapTopic table via ParentTopicId — a
/// section is just a topic with no parent and no mastery-bearing content of its own. Prerequisites
/// arrive from the model as title references (it has no ids to give), so they're resolved to real
/// RoadmapTopic ids in a second pass once every node in the tree has been assigned one.
/// </summary>
public static class RoadmapFinalizer
{
    public static async Task<RoadmapDto> FinalizeAsync(
        IApplicationDbContext db,
        Guid userId,
        RoadmapProfile profile,
        RoadmapGenerationResult data,
        AiTelemetryRecord telemetry,
        DateTime nowUtc,
        CancellationToken ct)
    {
        if (data.Sections.Count == 0)
            throw new AiGenerationFailedException("The AI returned a roadmap with no sections.");

        // Title and description alone get real fallbacks (cosmetic — shown once at the top of the
        // page — not a content-integrity problem the way a missing topic field would be). Given how
        // large a full roadmap generation is, the model occasionally omits one of these two while
        // still producing a complete, valid section/topic tree; failing the whole generation over
        // that isn't worth it.
        var title = string.IsNullOrWhiteSpace(data.Title) ? $"{profile.CareerGoal} Roadmap" : data.Title;
        var description = string.IsNullOrWhiteSpace(data.Description) ? $"A personalized learning roadmap for {profile.CareerGoal}." : data.Description;

        var roadmap = LearningRoadmap.Create(
            userId, profile.CareerGoal, title, description, ParseDifficulty(data.Difficulty), Math.Max(1, data.EstimatedWeeks),
            telemetry.Model, telemetry.PromptVersion ?? "v1", telemetry.CorrelationId, nowUtc);
        db.LearningRoadmaps.Add(roadmap);

        var allTopics = new List<RoadmapTopic>();
        var titleToId = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
        var pendingPrerequisites = new List<(RoadmapTopic Topic, IReadOnlyList<string> Titles)>();

        void Flatten(IReadOnlyList<RoadmapTopicResult> nodes, Guid? parentId)
        {
            var order = 0;
            foreach (var node in nodes)
            {
                var topic = RoadmapTopic.Create(
                    roadmap.Id, parentId, order++,
                    RequireField(node.Title, "topic.title"),
                    node.Description ?? string.Empty,
                    Math.Max(0, node.EstimatedHours),
                    ParseDifficulty(node.Difficulty),
                    SerializeResources(node.Resources),
                    SerializeStrings(node.SuggestedProjects),
                    nowUtc);

                allTopics.Add(topic);
                db.RoadmapTopics.Add(topic);
                titleToId.TryAdd(topic.Title, topic.Id);

                if (node.Prerequisites is { Count: > 0 })
                    pendingPrerequisites.Add((topic, node.Prerequisites));

                if (node.SubTopics is { Count: > 0 })
                    Flatten(node.SubTopics, topic.Id);
            }
        }

        var sectionOrder = 0;
        foreach (var section in data.Sections)
        {
            var sectionTopic = RoadmapTopic.Create(
                roadmap.Id, null, sectionOrder++,
                RequireField(section.Title, "section.title"),
                section.Description ?? string.Empty,
                0, RoadmapDifficultyLevel.Beginner, "[]", "[]", nowUtc);

            allTopics.Add(sectionTopic);
            db.RoadmapTopics.Add(sectionTopic);
            titleToId.TryAdd(sectionTopic.Title, sectionTopic.Id);

            Flatten(section.Topics, sectionTopic.Id);
        }

        foreach (var (topic, titles) in pendingPrerequisites)
        {
            var ids = titles
                .Select(t => titleToId.GetValueOrDefault(t.Trim()))
                .Where(id => id != Guid.Empty && id != topic.Id)
                .Distinct()
                .ToList();

            if (ids.Count > 0)
                topic.SetPrerequisites(JsonSerializer.Serialize(ids), nowUtc);
        }

        await db.SaveChangesAsync(ct);

        // A brand-new roadmap has no quiz history yet — an empty mastery map is correct, not a
        // shortcut; GetRoadmapQueryHandler re-computes with the real map on every subsequent read.
        return RoadmapProgressCalculator.BuildDto(roadmap, allTopics, new Dictionary<string, double>());
    }

    private static string RequireField(string? value, string fieldName) =>
        string.IsNullOrWhiteSpace(value) ? throw new AiGenerationFailedException($"The AI response was missing a required '{fieldName}' value.") : value;

    private static RoadmapDifficultyLevel ParseDifficulty(string raw) =>
        Enum.TryParse<RoadmapDifficultyLevel>(raw, ignoreCase: true, out var parsed)
            ? parsed
            : throw new AiGenerationFailedException($"Unrecognized difficulty '{raw}' in AI response.");

    private static string SerializeResources(IReadOnlyList<RoadmapResourceResult>? resources) =>
        JsonSerializer.Serialize((resources ?? []).Select(r => new RoadmapResourceDto(r.Type, r.Title, r.Url)));

    private static string SerializeStrings(IReadOnlyList<string>? values) =>
        JsonSerializer.Serialize(values ?? []);
}
