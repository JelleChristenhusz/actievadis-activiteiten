using Shared.DTOs;

namespace UI.Components.Activities;

/// <summary>Where an activity stands right now, seen from the overview page.</summary>
public enum ActivityStatus
{
    /// <summary>Not finished yet.</summary>
    Current,

    /// <summary>The end time has passed.</summary>
    Finished
}

public static class ActivityStatusExtensions
{
    public static ActivityStatus GetStatus(this ActivityDto activity, DateTime now)
    {
        return activity.EndTime < now ? ActivityStatus.Finished : ActivityStatus.Current;
    }
}
