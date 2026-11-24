using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Implementations;

/// <summary>Persists and retrieves authorization roles.</summary>
public sealed class RoleRepository(AppDbContext context) : Repository<Role, int>(context), IRoleRepository
{
    /// <inheritdoc />
    public async Task<Role?> GetByNameAsync(string name) =>
        await _dbSet.SingleOrDefaultAsync(role => role.Name == name);
}
