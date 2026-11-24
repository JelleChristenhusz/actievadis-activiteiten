using Shared.DTOs;
using API.Authentication;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

/// <summary>Exposes password-based authentication.</summary>
[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public sealed class AuthController(IAuthenticationService authenticationService, IJwtTokenService tokenService) : ControllerBase
{
    /// <summary>Validates credentials and returns a signed access token.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiSuccessResponse<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await authenticationService.AuthenticateAsync(request.Email, request.Password);
        if (user is null)
            return Unauthorized(ApiErrorResponse.Create("E-mailadres of wachtwoord is onjuist."));

        var token = tokenService.Create(user);
        return Ok(ApiSuccessResponse<LoginResponse>.Create(new LoginResponse
        {
            Id = user.Id,
            Name = user.Name,
            JobTitle = user.JobTitle,
            Email = user.Email,
            RoleId = user.RoleId,
            Role = user.Role,
            AccessToken = token.AccessToken,
            ExpiresAtUtc = token.ExpiresAtUtc
        }, "Inloggen gelukt."));
    }
}
