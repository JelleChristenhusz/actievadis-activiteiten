using API.Exceptions;
using Shared.DTOs;
using Application.DTOs;
using Application.Services.Interfaces;
using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

/// <summary>Exposes administrator-only role lookups.</summary>
[ApiController]
[Route("api/roles")]
[Authorize(Roles = RoleNames.Administrator)]
public sealed class RolesController(IRoleService roleService) : ControllerBase
{
    /// <summary>Gets all authorization roles.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiSuccessResponse<IEnumerable<RoleDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var roles = await roleService.GetAllAsync();
        return Ok(ApiSuccessResponse<IEnumerable<RoleDto>>.Create(roles));
    }

    /// <summary>Gets a role by its integer key.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiSuccessResponse<RoleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var role = await roleService.GetByIdAsync(id)
            ?? throw new EntityNotFoundException("Role", id);
        return Ok(ApiSuccessResponse<RoleDto>.Create(role));
    }
}
