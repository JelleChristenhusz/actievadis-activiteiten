namespace Application.Services.Interfaces;

/// <summary>Checks whether a token still matches the stored user and role.</summary>
public interface IAccessTokenValidator
{
    /// <summary>Checks that the GUID identifies an existing user with the token's role.</summary>
    Task<bool> IsCurrentAsync(Guid userId, string role);
}
