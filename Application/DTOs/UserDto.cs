namespace Application.DTOs;

/// <summary>Describes a user without exposing credentials or persistence navigation properties.</summary>
public sealed class UserDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string JobTitle { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public RoleDto Role { get; init; } = new();
}
