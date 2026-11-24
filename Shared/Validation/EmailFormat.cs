using System.Text.RegularExpressions;

namespace Shared.Validation;

/// <summary>
/// Stricter e-mail check than [EmailAddress], which only looks for an "@": the address also needs a domain
/// with an extension. So "test@test" is refused and "test@test.nl" or "naam@bedrijf.com" are accepted.
/// </summary>
public static class EmailFormat
{
    /// <summary>Something before the @, a domain after it, and an extension of at least two letters.</summary>
    public const string Pattern = @"^\s*[^@\s]+@[^@\s]+\.[A-Za-z]{2,}\s*$";

    public const string ErrorMessage = "Vul een geldig e-mailadres in, bijvoorbeeld naam@voorbeeld.nl.";

    public static bool IsValid(string? email) => email is not null && Regex.IsMatch(email, Pattern);
}
