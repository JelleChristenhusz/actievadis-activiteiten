using Shared.DTOs;

namespace Application.Services.Interfaces;

/// <summary>
/// Registering and unregistering visitors without an account (a "gast"), including the e-mails that go with it.
/// </summary>
public interface IVisitorRegistrationService
{
    /// <summary>
    /// Registers a visitor and e-mails the cancellation code. Returns null when the activity does not exist.
    /// </summary>
    Task<VisitorRegistrationResultDto?> RegisterAsync(Guid activityId, VisitorRegistrationDto visitor);

    /// <summary>
    /// Cancels a registration by e-mail address and code, and e-mails a confirmation. Returns null when nothing matches.
    /// </summary>
    Task<VisitorCancellationResultDto?> CancelAsync(CancelVisitorRegistrationDto request);
}
