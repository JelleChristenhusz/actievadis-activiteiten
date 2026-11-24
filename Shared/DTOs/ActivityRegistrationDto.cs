namespace Shared.DTOs;

public sealed class ActivityRegistrationDto
{
    public string ParticipantName { get; set; } = string.Empty;

    /// <summary>The participant's account has been deleted since; past participation is kept.</summary>
    public bool ParticipantDeleted { get; set; }

    public bool BringsGuest { get; set; }
    public string? Comment { get; set; }
    public DateTime RegisteredAt { get; set; }

    /// <summary>A visitor without an account (a "gast") instead of an employee.</summary>
    public bool IsVisitor { get; set; }

    /// <summary>Contact details of a visitor, so the organiser can reach them; null for employees.</summary>
    public string? VisitorEmail { get; set; }
    public string? VisitorPhone { get; set; }
}
