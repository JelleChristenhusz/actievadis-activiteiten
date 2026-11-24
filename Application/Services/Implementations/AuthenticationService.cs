using Application.DTOs;
using Application.Security;
using Application.Services.Interfaces;
using Infrastructure.Repositories.Interfaces;

namespace Application.Services.Implementations;

/// <summary>Authenticates users and upgrades legacy password hashes after successful login.</summary>
public sealed class AuthenticationService(IUserRepository users) : IAuthenticationService
{
    /// <inheritdoc />
    public async Task<AuthenticatedUserDto?> AuthenticateAsync(string email, string password)
    {
        var user = await users.GetByEmailWithRoleAsync(email.Trim().ToLowerInvariant());
        if (user is null || !PasswordHasher.Verify(password, user.PasswordHash, out var needsRehash))
            return null;

        if (needsRehash)
        {
            user.PasswordHash = PasswordHasher.Hash(password);
            await users.SaveChangesAsync();
        }

        return new AuthenticatedUserDto
        {
            Id = user.Id,
            Name = user.Name,
            JobTitle = user.JobTitle,
            Email = user.Email,
            RoleId = user.RoleId,
            Role = user.Role.Name
        };
    }
}
