using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Implementations;

/// <summary>Persists users and loads their authorization roles.</summary>
public sealed class UserRepository(AppDbContext context) : Repository<User, Guid>(context), IUserRepository
{
    /// <inheritdoc />
    public async Task<IEnumerable<User>> GetAllWithRoleAsync() =>
        await _dbSet.AsNoTracking().Include(user => user.Role).OrderBy(user => user.Name).ToListAsync();

    /// <inheritdoc />
    public async Task<User?> GetByIdWithRoleAsync(Guid id) =>
        await _dbSet.Include(user => user.Role).SingleOrDefaultAsync(user => user.Id == id);

    /// <inheritdoc />
    public async Task<User?> GetByIdWithRegistrationsAsync(Guid id) =>
        await _dbSet.Include(user => user.Role)
            .Include(user => user.Registrations).ThenInclude(registration => registration.Activity)
            .SingleOrDefaultAsync(user => user.Id == id);

    /// <inheritdoc />
    public void RemoveRegistrations(IEnumerable<Registration> registrations) =>
        _context.Registrations.RemoveRange(registrations);

    /// <inheritdoc />
    public async Task<User?> GetByEmailWithRoleAsync(string email) =>
        await _dbSet.Include(user => user.Role).SingleOrDefaultAsync(user => user.Email == email);

    /// <inheritdoc />
    public async Task<int> CountByRoleNameAsync(string roleName) =>
        await _dbSet.CountAsync(user => user.Role.Name == roleName);
}
