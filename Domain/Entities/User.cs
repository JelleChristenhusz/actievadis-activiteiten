using Domain.Common;

namespace Domain.Entities;

/// <summary>Represents a persisted user with a password hash and authorization role.</summary>
public class User : IEntity<Guid>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    /// <summary>One of <see cref="Constants.JobTitles"/>. It decides the role: Facilitair is an administrator.</summary>
    public string JobTitle { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public Role Role { get; set; } = null!;
    public List<Registration> Registrations { get; set; } = new();

    /// <summary>
    /// When the user was deleted, or null while the user is active. A deleted user stays in the database
    /// (so past participation is kept), but can no longer log in and is left out of every query.
    /// </summary>
    public DateTime? DeletedAt { get; set; }
}
