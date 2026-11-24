using Application.Exceptions;
using Application.Services.Interfaces;
using Domain.Constants;
using Infrastructure.Repositories.Interfaces;

namespace Application.Services.Implementations;

/// <summary>
/// Makes an explicitly selected existing account an administrator. The role follows from the job title,
/// so the account gets the job title Facilitair as well.
/// </summary>
public sealed class AdministratorBootstrapService(IUserRepository users, IRoleRepository roles)
    : IAdministratorBootstrapService
{
    /// <inheritdoc />
    public async Task BootstrapAsync(string email)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var user = await users.GetByEmailWithRoleAsync(normalizedEmail)
            ?? throw new ApplicationValidationException(
                "Bootstrap mislukt.", ["Er bestaat geen gebruiker met dit e-mailadres."]);

        var administrator = await roles.GetByNameAsync(RoleNames.Administrator)
            ?? throw new InvalidOperationException("The administrator role is missing from the database.");

        user.JobTitle = JobTitles.Facilitair;
        user.RoleId = administrator.Id;
        user.Role = administrator;
        await users.SaveChangesAsync();
    }
}
