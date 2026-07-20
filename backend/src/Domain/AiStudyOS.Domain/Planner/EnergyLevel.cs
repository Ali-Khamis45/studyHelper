namespace AiStudyOS.Domain.Planner;

/// <summary>
/// The AI's own estimate of how mentally demanding a task is — not a measurement of the student's
/// actual energy. Used to help sequence a day's tasks against typical energy patterns (e.g. favor
/// higher cognitive-load work earlier in the day).
/// </summary>
public enum EnergyLevel
{
    Low,
    Medium,
    High,
}
