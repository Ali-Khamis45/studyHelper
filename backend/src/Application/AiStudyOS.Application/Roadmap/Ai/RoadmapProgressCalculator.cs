using System.Text.Json;
using AiStudyOS.Application.Roadmap.Dtos;

namespace AiStudyOS.Application.Roadmap.Ai;

/// <summary>
/// Computes every topic's Status/MasteryScore/ProgressPercent at read time from three inputs this
/// module never writes to itself: the topic's own ManuallyCompletedAtUtc, its PrerequisiteTopicIds,
/// and the user's live Quiz TopicMastery rows. This is deliberate — "completing a quiz updates
/// roadmap progress" and "prerequisites gate unlocking" both fall out of this single read-time pass
/// instead of a write-time sync job that would have to hook into SubmitQuizCommandHandler.
///
/// Two passes, each a simple bottom-up/lookup pass (no topological sort needed):
///   1. Structural status — leaves from their own mastery/manual-complete; parents rolled up from
///      their children (a section is "Completed" once every child under it is).
///   2. Prerequisite lock overlay — a topic whose prerequisites aren't all Completed/Mastered is
///      forced to Locked, unless it's already Completed/Mastered itself (real progress is never
///      taken away).
/// </summary>
public static class RoadmapProgressCalculator
{
    public const double MasteredThreshold = 0.8;
    public const double CompletedThreshold = 0.6;

    private enum RawStatus { NotStarted, InProgress, Completed, Mastered }

    public static RoadmapDto BuildDto(
        Domain.Roadmap.LearningRoadmap roadmap,
        IReadOnlyList<Domain.Roadmap.RoadmapTopic> topics,
        IReadOnlyDictionary<string, double> masteryByTopicLower)
    {
        var (statuses, masteryScores) = ComputeStatuses(topics, masteryByTopicLower);
        var byParent = topics.ToLookup(t => t.ParentTopicId);
        var byId = topics.ToDictionary(t => t.Id);

        RoadmapTopicDto BuildNode(Domain.Roadmap.RoadmapTopic topic)
        {
            var children = byParent[topic.Id].OrderBy(c => c.Order).Select(BuildNode).ToList();
            var prerequisiteIds = DeserializeGuids(topic.PrerequisiteTopicIdsJson);

            return new RoadmapTopicDto(
                topic.Id,
                topic.ParentTopicId,
                topic.Order,
                topic.Title,
                topic.Description,
                topic.EstimatedHours,
                topic.Difficulty.ToString(),
                DeserializeResources(topic.ResourcesJson),
                DeserializeStrings(topic.SuggestedProjectsJson),
                prerequisiteIds
                    .Where(byId.ContainsKey)
                    .Select(id => new PrerequisiteStatusDto(id, byId[id].Title, statuses[id] is RawStatus.Completed or RawStatus.Mastered))
                    .ToList(),
                topic.LinkedMasteryTopic,
                masteryScores.GetValueOrDefault(topic.Id),
                ResolveDisplayStatus(topic.Id, statuses, prerequisiteIds),
                topic.ManuallyCompletedAtUtc is not null,
                topic.Notes,
                topic.UpdatedAtUtc,
                children);
        }

        var sections = byParent[null].OrderBy(t => t.Order).Select(BuildNode).ToList();
        var (completedLeaves, totalLeaves, totalHours, remainingHours) = SummarizeLeaves(topics, byParent, statuses);

        return new RoadmapDto(
            roadmap.Id,
            roadmap.CareerGoal,
            roadmap.Title,
            roadmap.Description,
            roadmap.Difficulty.ToString(),
            roadmap.EstimatedWeeks,
            roadmap.Status.ToString(),
            totalLeaves == 0 ? 0 : Math.Round(100.0 * completedLeaves / totalLeaves, 1),
            completedLeaves,
            totalLeaves,
            Math.Round(totalHours, 1),
            Math.Round(remainingHours, 1),
            roadmap.CreatedAtUtc,
            roadmap.UpdatedAtUtc,
            sections);
    }

    public static RoadmapSummaryDto BuildSummary(
        Domain.Roadmap.LearningRoadmap roadmap,
        IReadOnlyList<Domain.Roadmap.RoadmapTopic> topics,
        IReadOnlyDictionary<string, double> masteryByTopicLower)
    {
        var (statuses, _) = ComputeStatuses(topics, masteryByTopicLower);
        var byParent = topics.ToLookup(t => t.ParentTopicId);
        var (completedLeaves, totalLeaves, _, _) = SummarizeLeaves(topics, byParent, statuses);

        return new RoadmapSummaryDto(
            roadmap.Id,
            roadmap.CareerGoal,
            roadmap.Title,
            roadmap.Difficulty.ToString(),
            roadmap.EstimatedWeeks,
            roadmap.Status.ToString(),
            totalLeaves == 0 ? 0 : Math.Round(100.0 * completedLeaves / totalLeaves, 1),
            completedLeaves,
            totalLeaves,
            roadmap.CreatedAtUtc,
            roadmap.UpdatedAtUtc);
    }

    private static (int CompletedLeaves, int TotalLeaves, double TotalHours, double RemainingHours) SummarizeLeaves(
        IReadOnlyList<Domain.Roadmap.RoadmapTopic> topics,
        ILookup<Guid?, Domain.Roadmap.RoadmapTopic> byParent,
        Dictionary<Guid, RawStatus> statuses)
    {
        var leaves = topics.Where(t => !byParent[t.Id].Any()).ToList();
        var completed = leaves.Count(t => statuses[t.Id] is RawStatus.Completed or RawStatus.Mastered);
        var totalHours = leaves.Sum(t => t.EstimatedHours);
        var remainingHours = leaves.Where(t => statuses[t.Id] is not (RawStatus.Completed or RawStatus.Mastered)).Sum(t => t.EstimatedHours);
        return (completed, leaves.Count, totalHours, remainingHours);
    }

    private static (Dictionary<Guid, RawStatus> Statuses, Dictionary<Guid, double> Mastery) ComputeStatuses(
        IReadOnlyList<Domain.Roadmap.RoadmapTopic> topics, IReadOnlyDictionary<string, double> masteryByTopicLower)
    {
        var byParent = topics.ToLookup(t => t.ParentTopicId);
        var statuses = new Dictionary<Guid, RawStatus>();
        var masteryScores = new Dictionary<Guid, double>();

        // Post-order (children before parents) via recursion — trees generated in one pass are
        // shallow (a handful of levels), so plain recursion is simpler and safe here.
        RawStatus Resolve(Domain.Roadmap.RoadmapTopic topic)
        {
            var children = byParent[topic.Id].ToList();
            RawStatus status;

            if (children.Count > 0)
            {
                var childStatuses = children.Select(Resolve).ToList();
                status = childStatuses.All(s => s == RawStatus.Mastered) ? RawStatus.Mastered
                    : childStatuses.All(s => s is RawStatus.Completed or RawStatus.Mastered) ? RawStatus.Completed
                    : childStatuses.Any(s => s is not RawStatus.NotStarted) ? RawStatus.InProgress
                    : RawStatus.NotStarted;
            }
            else
            {
                var mastery = masteryByTopicLower.GetValueOrDefault(topic.LinkedMasteryTopic.Trim().ToLowerInvariant());
                masteryScores[topic.Id] = mastery;

                status = topic.ManuallyCompletedAtUtc is not null
                    ? (mastery >= MasteredThreshold ? RawStatus.Mastered : RawStatus.Completed)
                    : mastery >= MasteredThreshold ? RawStatus.Mastered
                    : mastery >= CompletedThreshold ? RawStatus.Completed
                    : mastery > 0 ? RawStatus.InProgress
                    : RawStatus.NotStarted;
            }

            statuses[topic.Id] = status;
            return status;
        }

        foreach (var root in byParent[null])
            Resolve(root);

        return (statuses, masteryScores);
    }

    private static string ResolveDisplayStatus(Guid topicId, Dictionary<Guid, RawStatus> statuses, IReadOnlyList<Guid> prerequisiteIds)
    {
        var status = statuses[topicId];
        if (status is RawStatus.Completed or RawStatus.Mastered)
            return status.ToString();

        var prerequisitesMet = prerequisiteIds.Count == 0
            || prerequisiteIds.All(id => statuses.TryGetValue(id, out var s) && s is RawStatus.Completed or RawStatus.Mastered);

        if (!prerequisitesMet)
            return "Locked";

        return status == RawStatus.NotStarted ? "Available" : status.ToString();
    }

    private static List<Guid> DeserializeGuids(string json) =>
        JsonSerializer.Deserialize<List<Guid>>(json) ?? [];

    private static List<string> DeserializeStrings(string json) =>
        JsonSerializer.Deserialize<List<string>>(json) ?? [];

    private static List<RoadmapResourceDto> DeserializeResources(string json) =>
        JsonSerializer.Deserialize<List<RoadmapResourceDto>>(json) ?? [];
}
