namespace Application.DTOs;

/// <summary>
/// Everything the confirmation e-mail to a visitor without an account needs, after registering for an activity.
/// </summary>
/// <param name="To">The visitor's e-mail address.</param>
/// <param name="VisitorName">The visitor's name, for the greeting.</param>
/// <param name="ActivityName">Name of the activity.</param>
/// <param name="Location">Where the activity takes place.</param>
/// <param name="StartTime">Start time (local time, as entered by the organiser).</param>
/// <param name="EndTime">End time (local time).</param>
/// <param name="BringsGuest">True when the visitor brings a +1.</param>
/// <param name="CancellationCode">The code the visitor needs, with the e-mail address, to unregister.</param>
/// <param name="CancellationPageUrl">The page where the visitor can unregister, or null when it is not configured.</param>
public sealed record VisitorRegistrationEmail(
    string To,
    string VisitorName,
    string ActivityName,
    string Location,
    DateTime StartTime,
    DateTime EndTime,
    bool BringsGuest,
    string CancellationCode,
    string? CancellationPageUrl);

/// <summary>Everything the e-mail to a visitor needs after unregistering.</summary>
/// <param name="To">The visitor's e-mail address.</param>
/// <param name="VisitorName">The visitor's name, for the greeting.</param>
/// <param name="ActivityName">Name of the activity the visitor unregistered from.</param>
/// <param name="StartTime">Start time of that activity (local time).</param>
public sealed record VisitorCancellationEmail(
    string To,
    string VisitorName,
    string ActivityName,
    DateTime StartTime);
