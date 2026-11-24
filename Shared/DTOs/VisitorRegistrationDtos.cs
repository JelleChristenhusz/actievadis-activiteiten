using System.ComponentModel.DataAnnotations;
using Shared.Validation;

namespace Shared.DTOs;

/// <summary>
/// What a visitor without an account (a "gast") fills in to register for an activity.
/// A visitor counts towards the maximum, just like an employee.
/// </summary>
public sealed class VisitorRegistrationDto
{
    [Required(ErrorMessage = "Vul je naam in."), MaxLength(200, ErrorMessage = "Je naam mag maximaal 200 tekens lang zijn.")]
    [RegularExpression(@".*\S.*", ErrorMessage = "Vul je naam in.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vul je e-mailadres in."), RegularExpression(EmailFormat.Pattern, ErrorMessage = EmailFormat.ErrorMessage)]
    [MaxLength(320, ErrorMessage = "Je e-mailadres mag maximaal 320 tekens lang zijn.")]
    public string Email { get; set; } = string.Empty;

    /// <summary>Digits with optional +, spaces, dashes and brackets, for example 06 12345678 or +31 6 12345678.</summary>
    [Required(ErrorMessage = "Vul je telefoonnummer in.")]
    [RegularExpression(@"^\s*\+?[0-9][0-9 ()\-]{7,24}\s*$", ErrorMessage = "Vul een geldig telefoonnummer in.")]
    public string Phone { get; set; } = string.Empty;

    /// <summary>Brings one +1. The +1 itself never has to register or give any details.</summary>
    public bool BringsGuest { get; set; }

    [MaxLength(1000, ErrorMessage = "Een opmerking mag maximaal 1000 tekens lang zijn.")]
    public string? Comment { get; set; }
}

/// <summary>The result of a visitor registration: the activity as it is now, and the code to unregister with.</summary>
public sealed class VisitorRegistrationResultDto
{
    public ActivityDto Activity { get; set; } = new();

    /// <summary>
    /// The code the visitor needs, together with the e-mail address, to unregister (for example "K7MQ-2XPA").
    /// It is only given out once: the database stores just a hash of it.
    /// </summary>
    public string CancellationCode { get; set; } = string.Empty;
}

/// <summary>What a visitor fills in to unregister.</summary>
public sealed class CancelVisitorRegistrationDto
{
    [Required(ErrorMessage = "Vul je e-mailadres in."), RegularExpression(EmailFormat.Pattern, ErrorMessage = EmailFormat.ErrorMessage)]
    [MaxLength(320)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vul je uitschrijfcode in."), MaxLength(20)]
    public string Code { get; set; } = string.Empty;
}

/// <summary>Which registration a visitor just cancelled.</summary>
public sealed class VisitorCancellationResultDto
{
    public Guid ActivityId { get; set; }
    public string ActivityName { get; set; } = string.Empty;
    public DateTime ActivityStartTime { get; set; }
    public string VisitorName { get; set; } = string.Empty;
}
