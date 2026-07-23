using System.Globalization;
using System.Text;
using AiStudyOS.Application.Analytics.Dtos;

namespace AiStudyOS.Application.Analytics.Export;

/// <summary>Renders the same AnalyticsOverviewDto every /analytics response already carries — export never recomputes anything.</summary>
public static class AnalyticsCsvExporter
{
    public static byte[] Generate(AnalyticsOverviewDto report)
    {
        var sb = new StringBuilder();
        var culture = CultureInfo.InvariantCulture;

        void Section(string title) => sb.AppendLine().AppendLine($"# {title}");
        void Row(string metric, object value) => sb.AppendLine($"{Escape(metric)},{Escape(Convert.ToString(value, culture) ?? string.Empty)}");

        sb.AppendLine($"AI Study OS — Analytics Report ({report.From:yyyy-MM-dd} to {report.To:yyyy-MM-dd})");

        Section("Study Time (minutes)");
        Row("Daily", report.StudyTime.DailyMinutes);
        Row("Weekly", report.StudyTime.WeeklyMinutes);
        Row("Monthly", report.StudyTime.MonthlyMinutes);

        Section("Tasks");
        Row("Completed", report.Tasks.Completed);
        Row("Skipped", report.Tasks.Skipped);
        Row("Rescheduled", report.Tasks.Rescheduled);
        Row("Pending", report.Tasks.Pending);
        Row("Total", report.Tasks.Total);
        Row("Completion Rate %", report.Tasks.CompletionRatePercent);

        Section("Goals");
        Row("Total Goals", report.Goals.TotalGoals);
        Row("Completed Goals", report.Goals.CompletedGoals);
        Row("Completion %", report.Goals.CompletionPercent);
        sb.AppendLine("Goal,Status,Progress %");
        foreach (var goal in report.Goals.Goals)
            sb.AppendLine($"{Escape(goal.Title)},{Escape(goal.Status)},{goal.ProgressPercent}");

        Section("Streak");
        Row("Current Streak", report.Streak.CurrentStreak);
        Row("Longest Streak", report.Streak.LongestStreak);

        Section("Quizzes");
        Row("Attempt Count", report.Quizzes.AttemptCount);
        Row("Average Score", report.Quizzes.AverageScore);
        Row("Highest Score", report.Quizzes.HighestScore);
        Row("Lowest Score", report.Quizzes.LowestScore);

        Section("Mastery — Weak Topics");
        sb.AppendLine("Topic,Mastery %,Attempts");
        foreach (var t in report.Mastery.WeakTopics)
            sb.AppendLine($"{Escape(t.Topic)},{Math.Round(t.MasteryScore * 100, 1)},{t.AttemptsCount}");

        Section("Mastery — Strong Topics");
        sb.AppendLine("Topic,Mastery %,Attempts");
        foreach (var t in report.Mastery.StrongTopics)
            sb.AppendLine($"{Escape(t.Topic)},{Math.Round(t.MasteryScore * 100, 1)},{t.AttemptsCount}");

        Section("Mentor");
        Row("Conversation Count", report.Mentor.ConversationCount);
        Row("Message Count", report.Mentor.MessageCount);
        Row("Average Session Length (min)", report.Mentor.AverageSessionLengthMinutes);

        Section("AI Usage");
        Row("Total Requests", report.Ai.TotalRequests);
        Row("Success Rate %", report.Ai.SuccessRatePercent);
        Row("Failure Rate %", report.Ai.FailureRatePercent);
        Row("Average Latency (ms)", report.Ai.AverageLatencyMs);
        Row("Prompt Tokens", report.Ai.TotalPromptTokens);
        Row("Completion Tokens", report.Ai.TotalCompletionTokens);

        Section("Planner Effectiveness");
        Row("Recommendation Count", report.Planner.RecommendationCount);
        Row("Acceptance Rate %", report.Planner.AcceptanceRatePercent);
        Row("Average Confidence", report.Planner.AverageConfidence);
        Row("Average Generation Time (ms)", report.Planner.AverageGenerationTimeMs);

        if (report.Insights is not null)
        {
            Section("AI Insights");
            Row("Weekly Summary", report.Insights.WeeklySummary);
            Row("Monthly Summary", report.Insights.MonthlySummary);
            Row("Risk Detection", report.Insights.RiskDetection);
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private static string Escape(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";

        return value;
    }
}
