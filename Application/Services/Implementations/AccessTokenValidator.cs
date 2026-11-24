using Application.Services.Interfaces;
using Infrastructure.Repositories.Interfaces;

namespace Application.Services.Implementations;

/// <summary>Rejects tokens for deleted users or changed roles.</summary>
public sealed class AccessTokenValidator(IUserRepository users) : IAccessTokenValidator
{
    /// <inheritdoc />
    public async Task<bool> IsCurrentAsync(Guid userId, string role)
    {
        var user = await users.GetByIdWithRoleAsync(userId);
        return user is not null && string.Equals(user.Role.Name, role, StringComparison.Ordinal);
    }
}
