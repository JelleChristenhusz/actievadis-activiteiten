namespace Application.DTOs;

/// <summary>Carries a verified identity from the application layer to token issuance.</summary>
public sealed class AuthenticatedUserDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string JobTitle { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public int RoleId { get; init; }
    public string Role { get; init; } = string.Empty;
}
