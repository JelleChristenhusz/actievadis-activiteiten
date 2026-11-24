using Application.DTOs;

namespace Application.Services.Interfaces;

/// <summary>Reads the available authorization roles.</summary>
public interface IRoleService
{
    /// <summary>Gets all available roles.</summary>
    Task<IEnumerable<RoleDto>> GetAllAsync();
    /// <summary>Gets a role by its stable integer key.</summary>
    Task<RoleDto?> GetByIdAsync(int id);
}
