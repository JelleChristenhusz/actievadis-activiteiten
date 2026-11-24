namespace Shared.DTOs;

/// <summary>Contains the authenticated identity and its issued access token.</summary>
public sealed class LoginResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string JobTitle { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public int RoleId { get; init; }
    public string Role { get; init; } = string.Empty;
    public string AccessToken { get; init; } = string.Empty;
    public DateTime ExpiresAtUtc { get; init; }
}
