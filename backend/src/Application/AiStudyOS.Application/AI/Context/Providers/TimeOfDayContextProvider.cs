using AiStudyOS.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AiStudyOS.Application.AI.Context.Providers;

/// <summary>
/// Gives the Recommendation agent the student's current local time-of-day bucket (using their
/// stored IANA time zone, not server UTC) plus brief, general energy-sequencing guidance, so it can
/// favor higher cognitive-load tasks earlier in the day and lighter review work later — real,
/// model-reasoned energy-awareness rather than a hardcoded rule.
/// </summary>
public class TimeOfDayContextProvider(IApplicationDbContext db, IDateTimeProvider dateTimeProvider) : IContextProvider
{
    public string SectionName => "Current Time Context";

    public async Task<ContextFragment> BuildAsync(ContextRequest request, CancellationToken ct)
    {
        var timeZoneId = await db.Users
            .Where(u => u.Id == request.UserId)
            .Select(u => u.TimeZone)
            .FirstOrDefaultAsync(ct) ?? "UTC";

        var localHour = ResolveLocalHour(dateTimeProvider.UtcNow, timeZoneId);

        var (bucket, guidance) = localHour switch
        {
            >= 5 and < 12 => ("Morning", "Cognitive capacity is typically highest in the morning — favor higher energyLevel (High) tasks that need sustained focus or new material."),
            >= 12 and < 17 => ("Afternoon", "A moderate-energy window — a mix of energyLevel is reasonable, but avoid stacking multiple High-energy tasks back to back."),
            >= 17 and < 22 => ("Evening", "Energy is typically lower in the evening — favor lower energyLevel (Low/Medium) tasks like review, flashcards, or light reading over demanding new material."),
            _ => ("Late night", "It's late — if the student is planning at this hour, favor lower energyLevel (Low) tasks and keep the plan light."),
        };

        var content = $"Student's local time of day: {bucket} (local hour: {localHour}). {guidance}";
        return new ContextFragment(SectionName, content, EstimatedTokens: content.Length / 4, Priority: 60);
    }

    private static int ResolveLocalHour(DateTime utcNow, string timeZoneId)
    {
        try
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return TimeZoneInfo.ConvertTimeFromUtc(utcNow, timeZone).Hour;
        }
        catch (TimeZoneNotFoundException)
        {
            return utcNow.Hour;
        }
        catch (InvalidTimeZoneException)
        {
            return utcNow.Hour;
        }
    }
}
