namespace Application.Services.Interfaces;

/// <summary>Promotes an existing user to administrator during explicit bootstrap.</summary>
public interface IAdministratorBootstrapService
{
    /// <summary>Gives the existing account with this email address the job title Facilitair and the administrator role.</summary>
    Task BootstrapAsync(string email);
}
