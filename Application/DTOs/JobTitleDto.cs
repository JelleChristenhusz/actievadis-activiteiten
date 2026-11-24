namespace Application.DTOs;

/// <summary>A job title a user can have, and whether it gives administrator rights.</summary>
public sealed class JobTitleDto
{
    public string Name { get; init; } = string.Empty;
    public bool IsAdministrator { get; init; }
}
