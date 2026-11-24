namespace Application.DTOs;

/// <summary>Describes a public authorization role.</summary>
public sealed class RoleDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}
