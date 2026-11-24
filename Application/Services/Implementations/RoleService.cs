using Application.DTOs;
using Application.Services.Interfaces;
using Domain.Entities;
using Infrastructure.Repositories.Interfaces;

namespace Application.Services.Implementations;

/// <summary>Maps persisted roles to public role DTOs.</summary>
public sealed class RoleService(IRoleRepository roles) : IRoleService
{
    /// <inheritdoc />
    public async Task<IEnumerable<RoleDto>> GetAllAsync() =>
        (await roles.GetAllAsync()).OrderBy(role => role.Id).Select(Map);

    /// <inheritdoc />
    public async Task<RoleDto?> GetByIdAsync(int id)
    {
        var role = await roles.GetByIdAsync(id);
        return role is null ? null : Map(role);
    }

    private static RoleDto Map(Role role) => new()
    {
        Id = role.Id,
        Name = role.Name,
        Description = role.Description
    };
}
