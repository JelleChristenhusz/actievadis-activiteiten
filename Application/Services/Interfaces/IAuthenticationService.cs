using Application.DTOs;

namespace Application.Services.Interfaces;

/// <summary>Validates credentials and returns the authenticated user's public identity.</summary>
public interface IAuthenticationService
{
    /// <summary>Validates credentials, upgrades outdated hashes and returns null on failure.</summary>
    Task<AuthenticatedUserDto?> AuthenticateAsync(string email, string password);
}
