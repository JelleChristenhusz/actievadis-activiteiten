using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using API.Exceptions;
using Shared.DTOs;
using Application.DTOs;
using Application.Services.Interfaces;
using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

/// <summary>
/// Exposes administrator-only user management. There is no separate role assignment:
/// the role follows from the job title (Facilitair is an administrator).
/// </summary>
[ApiController]
[Route("api/users")]
[Authorize(Roles = RoleNames.Administrator)]
public sealed class UsersController(IUserService userService) : ControllerBase
{
    /// <summary>Gets all users without credentials.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiSuccessResponse<IEnumerable<UserDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var users = await userService.GetAllAsync();
        return Ok(ApiSuccessResponse<IEnumerable<UserDto>>.Create(users));
    }

    /// <summary>Gets the fixed job titles a user can have, and which of them gives administrator rights.</summary>
    [HttpGet("job-titles")]
    [ProducesResponseType(typeof(ApiSuccessResponse<IEnumerable<JobTitleDto>>), StatusCodes.Status200OK)]
    public IActionResult GetJobTitles() =>
        Ok(ApiSuccessResponse<IEnumerable<JobTitleDto>>.Create(userService.GetJobTitles()));

    /// <summary>Gets a user by GUID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiSuccessResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var user = await userService.GetByIdAsync(id)
            ?? throw new EntityNotFoundException("User", id);
        return Ok(ApiSuccessResponse<UserDto>.Create(user));
    }

    /// <summary>Creates a user; the role follows from the job title.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiSuccessResponse<UserDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
    {
        var user = await userService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = user.Id },
            ApiSuccessResponse<UserDto>.Create(user, "Gebruiker aangemaakt."));
    }

    /// <summary>Updates a user's name, email address, job title (and with it the role) and optional password.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiSuccessResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserDto dto)
    {
        var user = await userService.UpdateAsync(id, dto, CurrentUserId)
            ?? throw new EntityNotFoundException("User", id);
        return Ok(ApiSuccessResponse<UserDto>.Create(user, "Gebruiker bijgewerkt."));
    }

    /// <summary>
    /// Soft deletes a user: they can no longer log in and disappear from every list. Registrations for activities
    /// that have not started yet are removed; past ones are kept. You cannot delete yourself or the last administrator.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (!await userService.DeleteAsync(id, CurrentUserId))
            throw new EntityNotFoundException("User", id);
        return NoContent();
    }

    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
}
