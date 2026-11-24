using Shared.DTOs;

namespace UI.Service;

public static class LoginResponseExtensions
{
    // Same value as Domain.Constants.RoleNames.Administrator in the API; the API sends it in LoginResponse.Role.
    private const string AdministratorRole = "Administrator";

    /// <summary>
    /// True when the logged in user is an administrator. Only used to decide what to show;
    /// the API checks the role again on every call.
    /// </summary>
    public static bool IsAdministrator(this LoginResponse user) =>
        string.Equals(user.Role, AdministratorRole, StringComparison.OrdinalIgnoreCase);
}
