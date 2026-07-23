using System.Text;
using AiStudyOS.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AiStudyOS.Application.AI.Context.Providers;

/// <summary>Recent past questions (so the model avoids repeating them) and recent attempt scores.</summary>
public class QuizHistoryContextProvider(IApplicationDbContext db) : IContextProvider
{
    private const int RecentQuizzes = 5;

    public string SectionName => "Recent Quiz History";

    public async Task<ContextFragment> BuildAsync(ContextRequest request, CancellationToken ct)
    {
        var recentQuizIds = await db.Quizzes
            .Where(q => q.UserId == request.UserId)
            .OrderByDescending(q => q.CreatedAtUtc)
            .Take(RecentQuizzes)
            .Select(q => q.Id)
            .ToListAsync(ct);

        if (recentQuizIds.Count == 0)
        {
            const string empty = "No previous quizzes — this is the student's first one.";
            return new ContextFragment(SectionName, empty, EstimatedTokens: empty.Length / 4, Priority: 50);
        }

        var recentQuestions = await db.QuizQuestions
            .Where(q => recentQuizIds.Contains(q.QuizId))
            .Select(q => q.Text)
            .ToListAsync(ct);

        var sb = new StringBuilder();
        sb.AppendLine("Questions already asked recently — do not repeat these or trivial rewordings of them:");
        foreach (var text in recentQuestions)
            sb.AppendLine($"- {text}");

        var content = sb.ToString().TrimEnd();
        return new ContextFragment(SectionName, content, EstimatedTokens: content.Length / 4, Priority: 50);
    }
}
