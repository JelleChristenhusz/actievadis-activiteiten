using API.Exceptions;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs;

namespace API.Controllers;

/// <summary>
/// Registering and unregistering for visitors without an account (a "gast"). No login is needed:
/// a visitor gives a name, e-mail address and phone number, and unregisters with the e-mail address and a code.
/// </summary>
[ApiController]
[Route("api")]
[AllowAnonymous]
public sealed class VisitorRegistrationsController(IVisitorRegistrationService visitors) : ControllerBase
{
    /// <summary>
    /// Registers a visitor for an activity. The visitor counts towards the maximum. The response contains the
    /// cancellation code; it is also e-mailed to the visitor.
    /// </summary>
    [HttpPost("activities/{id:guid}/visitor-registrations")]
    [ProducesResponseType(typeof(ApiSuccessResponse<VisitorRegistrationResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(Guid id, [FromBody] VisitorRegistrationDto request)
    {
        var result = await visitors.RegisterAsync(id, request)
            ?? throw new EntityNotFoundException("Deze activiteit bestaat niet (meer).");
        return Ok(ApiSuccessResponse<VisitorRegistrationResultDto>.Create(result, "Je bent ingeschreven."));
    }

    /// <summary>Cancels a visitor registration with the e-mail address and the cancellation code.</summary>
    [HttpPost("visitor-registrations/cancel")]
    [ProducesResponseType(typeof(ApiSuccessResponse<VisitorCancellationResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancel([FromBody] CancelVisitorRegistrationDto request)
    {
        var result = await visitors.CancelAsync(request);
        if (result is null)
            return NotFound(ApiErrorResponse.Create("We vinden geen inschrijving met dit e-mailadres en deze code."));
        return Ok(ApiSuccessResponse<VisitorCancellationResultDto>.Create(result, "Je bent uitgeschreven."));
    }
}
