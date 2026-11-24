using Domain.Entities;

namespace Infrastructure.Repositories.Interfaces;

/// <summary>Loads persisted authorization roles.</summary>
public interface IRoleRepository : IRepository<Role, int>
{
    /// <summary>Gets a role by its canonical English name.</summary>
    Task<Role?> GetByNameAsync(string name);
}
