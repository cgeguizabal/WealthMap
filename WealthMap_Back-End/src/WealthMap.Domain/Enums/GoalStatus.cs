namespace WealthMap.Domain.Enums;

/// <summary>
/// Computed on read, never stored: derived from amounts, deadline and elapsed time.
/// </summary>
public enum GoalStatus
{
    OnTrack = 1,
    BehindSchedule = 2,
    DeadlinePassed = 3,
    Completed = 4
}
