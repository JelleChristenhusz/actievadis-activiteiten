using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs;

/// <summary>Contains the credentials submitted to the authentication API.</summary>
public sealed class LoginRequest
{
    [Required(ErrorMessage = "Vul je e-mailadres in.")]
    [EmailAddress(ErrorMessage = "Vul een geldig e-mailadres in.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vul je wachtwoord in.")]
    [MinLength(8, ErrorMessage = "Het wachtwoord moet minimaal 8 tekens bevatten.")]
    public string Password { get; set; } = string.Empty;
}
