using Domain.Entities;

namespace Infrastructure.Repositories.Interfaces;

/// <summary>Loads users and their roles for internal persistence operations.</summary>
public interface IUserRepository : IRepository<User, Guid>
{
    /// <summary>Gets untracked users with their roles, ordered by name.</summary>
    Task<IEnumerable<User>> GetAllWithRoleAsync();
    /// <summary>Gets a tracked user and role by GUID.</summary>
    Task<User?> GetByIdWithRoleAsync(Guid id);
    /// <summary>Gets a tracked user with role, registrations and the activities of those registrations.</summary>
    Task<User?> GetByIdWithRegistrationsAsync(Guid id);
    /// <summary>Marks registrations for deletion; they are removed on the next save.</summary>
    void RemoveRegistrations(IEnumerable<Registration> registrations);
    /// <summary>Gets a tracked user and role by normalized email address.</summary>
    Task<User?> GetByEmailWithRoleAsync(string email);
    /// <summary>Counts users assigned to the specified role.</summary>
    Task<int> CountByRoleNameAsync(string roleName);
}
