using Application.DTOs;
using Application.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.DTOs;

namespace Application.Services.Implementations;

/// <summary>Settings for the e-mails to visitors without an account (section "VisitorEmails" in appsettings).</summary>
public sealed class VisitorEmailOptions
{
    public const string SectionName = "VisitorEmails";

    /// <summary>The website page where visitors unregister, for the link in the e-mail.</summary>
    public string? CancellationPageUrl { get; set; }
}

/// <summary>
/// Registers and unregisters visitors without an account through <see cref="IActivityService"/>, which holds the rules,
/// and then e-mails them. A failed e-mail is logged but does not undo the registration: the website also shows the code.
/// </summary>
public sealed class VisitorRegistrationService(
    IActivityService activities,
    IVisitorEmailSender emails,
    IOptions<VisitorEmailOptions> options,
    ILogger<VisitorRegistrationService> logger) : IVisitorRegistrationService
{
    /// <inheritdoc />
    public async Task<VisitorRegistrationResultDto?> RegisterAsync(Guid activityId, VisitorRegistrationDto visitor)
    {
        var result = await activities.RegisterVisitorAsync(activityId, visitor);
        if (result is null)
            return null;

        var activity = result.Activity;
        await SendAsync(() => emails.SendRegistrationConfirmationAsync(new VisitorRegistrationEmail(
            To: visitor.Email.Trim().ToLowerInvariant(),
            VisitorName: visitor.Name.Trim(),
            ActivityName: activity.Name,
            Location: activity.Location,
            StartTime: activity.StartTime,
            EndTime: activity.EndTime,
            BringsGuest: visitor.BringsGuest,
            CancellationCode: result.CancellationCode,
            CancellationPageUrl: options.Value.CancellationPageUrl)), "registration confirmation");
        return result;
    }

    /// <inheritdoc />
    public async Task<VisitorCancellationResultDto?> CancelAsync(CancelVisitorRegistrationDto request)
    {
        var result = await activities.CancelVisitorRegistrationAsync(request.Email, request.Code);
        if (result is null)
            return null;

        await SendAsync(() => emails.SendCancellationConfirmationAsync(new VisitorCancellationEmail(
            To: request.Email.Trim().ToLowerInvariant(),
            VisitorName: result.VisitorName,
            ActivityName: result.ActivityName,
            StartTime: result.ActivityStartTime)), "cancellation confirmation");
        return result;
    }

    private async Task SendAsync(Func<Task> send, string kind)
    {
        try
        {
            await send();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Could not send the visitor {Kind} e-mail.", kind);
        }
    }
}
