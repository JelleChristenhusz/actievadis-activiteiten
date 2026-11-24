using Application.DTOs;

namespace Application.Services.Interfaces;

/// <summary>
/// Delivers the e-mails to visitors without an account. The application already decides when a mail is sent and
/// what goes in it; an implementation only turns the data into a mail (the template) and sends it.
/// A failure may throw: the caller logs it, and the registration itself stays saved.
/// </summary>
public interface IVisitorEmailSender
{
    /// <summary>Sent right after a visitor registered. Must contain the cancellation code.</summary>
    Task SendRegistrationConfirmationAsync(VisitorRegistrationEmail email);

    /// <summary>Sent right after a visitor unregistered.</summary>
    Task SendCancellationConfirmationAsync(VisitorCancellationEmail email);
}
