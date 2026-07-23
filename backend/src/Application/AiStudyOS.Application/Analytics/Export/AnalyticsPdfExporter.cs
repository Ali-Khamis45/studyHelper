using AiStudyOS.Application.Analytics.Dtos;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AiStudyOS.Application.Analytics.Export;

/// <summary>Renders the same AnalyticsOverviewDto every /analytics response already carries — export never recomputes anything. QuestPDF.Settings.License is set once at startup (see DependencyInjection).</summary>
public static class AnalyticsPdfExporter
{
    public static byte[] Generate(AnalyticsOverviewDto report)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(36);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Column(col =>
                {
                    col.Item().Text("AI Study OS — Analytics Report").FontSize(20).Bold();
                    col.Item().Text($"{report.From:MMM d, yyyy} – {report.To:MMM d, yyyy}").FontSize(10).FontColor(Colors.Grey.Darken1);
                });

                page.Content().PaddingVertical(12).Column(col =>
                {
                    col.Spacing(14);

                    col.Item().Element(c => Section(c, "Study Time", [
                        ("Daily", $"{report.StudyTime.DailyMinutes} min"),
                        ("Weekly", $"{report.StudyTime.WeeklyMinutes} min"),
                        ("Monthly", $"{report.StudyTime.MonthlyMinutes} min"),
                    ]));

                    col.Item().Element(c => Section(c, "Tasks", [
                        ("Completed", report.Tasks.Completed.ToString()),
                        ("Skipped", report.Tasks.Skipped.ToString()),
                        ("Rescheduled", report.Tasks.Rescheduled.ToString()),
                        ("Completion Rate", $"{report.Tasks.CompletionRatePercent}%"),
                    ]));

                    col.Item().Element(c => Section(c, "Goals", [
                        ("Total", report.Goals.TotalGoals.ToString()),
                        ("Completed", report.Goals.CompletedGoals.ToString()),
                        ("Completion %", $"{report.Goals.CompletionPercent}%"),
                    ]));

                    col.Item().Element(c => Section(c, "Streak", [
                        ("Current", $"{report.Streak.CurrentStreak} days"),
                        ("Longest", $"{report.Streak.LongestStreak} days"),
                    ]));

                    col.Item().Element(c => Section(c, "Quizzes", [
                        ("Attempts", report.Quizzes.AttemptCount.ToString()),
                        ("Average Score", $"{report.Quizzes.AverageScore}%"),
                        ("Highest", $"{report.Quizzes.HighestScore}%"),
                        ("Lowest", $"{report.Quizzes.LowestScore}%"),
                    ]));

                    col.Item().Element(c => Section(c, "Mentor", [
                        ("Conversations", report.Mentor.ConversationCount.ToString()),
                        ("Messages", report.Mentor.MessageCount.ToString()),
                        ("Avg Session", $"{report.Mentor.AverageSessionLengthMinutes} min"),
                    ]));

                    col.Item().Element(c => Section(c, "AI Usage", [
                        ("Requests", report.Ai.TotalRequests.ToString()),
                        ("Success Rate", $"{report.Ai.SuccessRatePercent}%"),
                        ("Avg Latency", $"{report.Ai.AverageLatencyMs} ms"),
                        ("Tokens", $"{report.Ai.TotalPromptTokens + report.Ai.TotalCompletionTokens}"),
                    ]));

                    col.Item().Element(c => Section(c, "Planner Effectiveness", [
                        ("Recommendations", report.Planner.RecommendationCount.ToString()),
                        ("Acceptance Rate", $"{report.Planner.AcceptanceRatePercent}%"),
                        ("Avg Confidence", report.Planner.AverageConfidence.ToString("0.00")),
                    ]));

                    if (report.Mastery.WeakTopics.Count > 0)
                    {
                        col.Item().Text("Weak Topics").Bold().FontSize(12);
                        col.Item().Text(string.Join(", ", report.Mastery.WeakTopics.Select(t => $"{t.Topic} ({Math.Round(t.MasteryScore * 100)}%)")));
                    }

                    if (report.Insights is not null)
                    {
                        col.Item().Text("AI Insights").Bold().FontSize(12);
                        col.Item().Text($"Weekly: {report.Insights.WeeklySummary}");
                        col.Item().Text($"Monthly: {report.Insights.MonthlySummary}");
                        if (report.Insights.Strengths.Count > 0)
                            col.Item().Text($"Strengths: {string.Join("; ", report.Insights.Strengths)}");
                        if (report.Insights.Weaknesses.Count > 0)
                            col.Item().Text($"Weaknesses: {string.Join("; ", report.Insights.Weaknesses)}");
                        col.Item().Text($"Risk: {report.Insights.RiskDetection}");
                    }
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Generated ").FontColor(Colors.Grey.Darken1);
                    x.Span(DateTime.UtcNow.ToString("u")).FontColor(Colors.Grey.Darken1);
                });
            });
        });

        return document.GeneratePdf();
    }

    private static void Section(IContainer container, string title, (string Label, string Value)[] rows)
    {
        container.Column(col =>
        {
            col.Item().Text(title).Bold().FontSize(12);
            col.Item().Row(row =>
            {
                foreach (var (label, value) in rows)
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text(label).FontSize(8).FontColor(Colors.Grey.Darken1);
                        c.Item().Text(value).FontSize(12).Bold();
                    });
                }
            });
        });
    }
}
