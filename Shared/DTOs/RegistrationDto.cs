using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs;

public sealed class RegistrationDto
{
    public bool IsRegistered { get; set; }
    public bool BringsGuest { get; set; }
    public string? Comment { get; set; }
}

/// <summary>
/// What a participant fills in when registering, and again when changing the registration afterwards.
/// </summary>
public sealed class RegisterActivityDto
{
    /// <summary>Brings one +1. A +1 cannot bring a +1 of their own, so there is never more than one.</summary>
    public bool BringsGuest { get; set; }

    [MaxLength(1000, ErrorMessage = "Een opmerking mag maximaal 1000 tekens lang zijn.")]
    public string? Comment { get; set; }
}
