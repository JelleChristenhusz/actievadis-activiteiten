using Application.DTOs;
using Application.Services.Interfaces;

namespace API.Email;

/// <summary>
/// Placeholder until e-mail is set up: it does not send anything, it only writes to the log that a mail would go out.
///
/// To send real e-mails, write a class that implements <see cref="IVisitorEmailSender"/> (mail server or mail service,
/// plus the template) and register it in Program.cs instead of this one. Everything else is ready: the application
/// decides when a mail is needed and passes all its data, including the cancellation code and the link to the
/// cancellation page (setting "VisitorEmails:CancellationPageUrl").
/// </summary>
public sealed class LoggingVisitorEmailSender(ILogger<LoggingVisitorEmailSender> logger) : IVisitorEmailSender
{
    /// <inheritdoc />
    public Task SendRegistrationConfirmationAsync(VisitorRegistrationEmail email)
    {
        // The code and address are left out of the log on purpose: logs are not a safe place for them.
        logger.LogInformation(
            "No e-mail sent (no mail service yet): registration confirmation for activity {ActivityName}.",
            email.ActivityName);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task SendCancellationConfirmationAsync(VisitorCancellationEmail email)
    {
        logger.LogInformation(
            "No e-mail sent (no mail service yet): cancellation confirmation for activity {ActivityName}.",
            email.ActivityName);
        return Task.CompletedTask;
    }
}
