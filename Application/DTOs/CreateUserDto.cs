using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

/// <summary>
/// Contains validated input for creating a user with a required password.
/// There is no role: it follows from the job title (Facilitair is an administrator).
/// </summary>
public sealed class CreateUserDto
{
    [Required, MaxLength(200), RegularExpression(@".*\S.*", ErrorMessage = "Naam mag niet alleen uit spaties bestaan.")]
    public string Name { get; init; } = string.Empty;

    /// <summary>One of the fixed job titles (see GET api/users/job-titles).</summary>
    [Required(ErrorMessage = "Kies een functie."), MaxLength(100)]
    public string JobTitle { get; init; } = string.Empty;

    [Required, EmailAddress, MaxLength(320)]
    public string Email { get; init; } = string.Empty;

    [Required, MinLength(8)]
    public string Password { get; init; } = string.Empty;
}
