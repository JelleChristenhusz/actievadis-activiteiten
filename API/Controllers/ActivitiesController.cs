using Domain.Constants;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs;
using Application.Services.Interfaces;
using API.Exceptions;

namespace API.Controllers
{
    /// <summary>Exposes activity management and searches for authenticated users.</summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Every call needs a valid JWT; get one through POST api/auth/login.
    public class ActivitiesController : ControllerBase
    {
        private readonly IActivityService _activityService;

        public ActivitiesController(IActivityService activityService)
        {
            _activityService = activityService ?? throw new ArgumentNullException(nameof(activityService));
        }

        /// <summary>
        /// Gets the activities, sorted by start time. Open to everyone, also without logging in, so visitors
        /// without an account can register. Visitors and normal users only get current activities;
        /// administrators also get finished ones.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiSuccessResponse<IEnumerable<ActivityDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllActivities()
        {
            var activities = User.IsInRole(RoleNames.Administrator)
                ? await _activityService.GetAllActivitiesForAdministratorAsync()
                : await _activityService.GetVisibleActivitiesAsync();

            return Ok(ApiSuccessResponse<IEnumerable<ActivityDto>>.Create(activities));
        }

        /// <summary>
        /// Gets the activities the authenticated user is registered for.
        /// </summary>
        [HttpGet("mine")]
        [ProducesResponseType(typeof(ApiSuccessResponse<IEnumerable<ActivityDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMyActivities()
        {
            var userIdValue = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (!Guid.TryParse(userIdValue, out var userId))
                return Unauthorized(ApiErrorResponse.Create("Ongeldig toegangstoken."));

            var activities = await _activityService.GetActivitiesForUserAsync(userId);
            return Ok(ApiSuccessResponse<IEnumerable<ActivityDto>>.Create(activities));
        }

        /// <summary>
        /// Gets an activity by ID. Open to everyone, also without logging in.
        /// </summary>
        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiSuccessResponse<ActivityDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetActivityById(Guid id)
        {
            var activity = await _activityService.GetActivityByIdAsync(id);
            if (activity == null)
                throw new EntityNotFoundException("Activity", id);

            return Ok(ApiSuccessResponse<ActivityDto>.Create(activity));
        }

        /// <summary>
        /// Creates a new activity.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = RoleNames.Administrator)]
        [ProducesResponseType(typeof(ApiSuccessResponse<ActivityDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateActivity([FromBody] ActivityInputDto createDto)
        {
            var activity = await _activityService.CreateActivityAsync(createDto);
            return CreatedAtAction(nameof(GetActivityById), new { id = activity.Id },
                ApiSuccessResponse<ActivityDto>.Create(activity, "Resource created successfully"));
        }

        /// <summary>
        /// Stores a photo for an activity and returns its address. Only administrators create activities,
        /// so only they may upload photos.
        /// </summary>
        [HttpPost("upload-image")]
        [Authorize(Roles = RoleNames.Administrator)]
        [ProducesResponseType(typeof(ApiSuccessResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(ApiErrorResponse.Create("Er is geen afbeelding gekozen."));

            const long maxFileSize = 5 * 1024 * 1024;

            if (file.Length > maxFileSize)
                return BadRequest(ApiErrorResponse.Create("De afbeelding mag maximaal 5 MB groot zijn."));

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
                return BadRequest(ApiErrorResponse.Create(
                    "Alleen JPG, JPEG, PNG en WEBP afbeeldingen zijn toegestaan."));

            var uploadsFolder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "uploads");

            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            await using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            var imageUrl =
                $"{Request.Scheme}://{Request.Host}/uploads/{fileName}";

            return Ok(ApiSuccessResponse<string>.Create(
                imageUrl,
                "Afbeelding succesvol geüpload."));
        }

        /// <summary>
        /// Updates an existing activity.
        /// </summary>
        [HttpPut("{id:guid}")]
        [Authorize(Roles = RoleNames.Administrator)]
        [ProducesResponseType(typeof(ApiSuccessResponse<ActivityDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateActivity(Guid id, [FromBody] ActivityInputDto updateDto)
        {
            var activity = await _activityService.UpdateActivityAsync(id, updateDto);
            if (activity == null)
                throw new EntityNotFoundException("Activity", id);

            return Ok(ApiSuccessResponse<ActivityDto>.Create(activity, "Resource updated successfully"));
        }

        /// <summary>
        /// Deletes an activity.
        /// </summary>
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = RoleNames.Administrator)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteActivity(Guid id)
        {
            var result = await _activityService.DeleteActivityAsync(id);
            if (!result)
                throw new EntityNotFoundException("Activity", id);

            return NoContent();
        }

        [HttpGet("{id:guid}/registrations")]
        [Authorize(Roles = RoleNames.Administrator)]
        public async Task<IActionResult> GetActivityRegistrations(Guid id)
        {
            if (await _activityService.GetActivityByIdAsync(id) is null)
                throw new EntityNotFoundException("Activity", id);
            var registrations = await _activityService.GetActivityRegistrationsAsync(id);
            return Ok(ApiSuccessResponse<IEnumerable<ActivityRegistrationDto>>.Create(registrations));
        }

        [HttpGet("{id:guid}/registration")]
        public async Task<IActionResult> GetRegistration(Guid id)
        {
            if (await _activityService.GetActivityByIdAsync(id) is null)
                throw new EntityNotFoundException("Activity", id);
            return Ok(ApiSuccessResponse<RegistrationDto>.Create(await _activityService.GetRegistrationAsync(id, CurrentUserId)));
        }

        [HttpPost("{id:guid}/registration")]
        public async Task<IActionResult> Register(Guid id, [FromBody] RegisterActivityDto? request)
        {
            var activity = await _activityService.RegisterAsync(id, CurrentUserId, request?.BringsGuest ?? false, request?.Comment);
            if (activity is null) throw new EntityNotFoundException("Activity", id);
            return Ok(ApiSuccessResponse<ActivityDto>.Create(activity, "Je bent ingeschreven."));
        }

        /// <summary>
        /// Changes the +1 and the comment of the authenticated user's registration, until the activity starts.
        /// </summary>
        [HttpPut("{id:guid}/registration")]
        [ProducesResponseType(typeof(ApiSuccessResponse<ActivityDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> UpdateRegistration(Guid id, [FromBody] RegisterActivityDto request)
        {
            if (await _activityService.GetActivityByIdAsync(id) is null)
                throw new EntityNotFoundException("Activity", id);
            var activity = await _activityService.UpdateRegistrationAsync(id, CurrentUserId, request.BringsGuest, request.Comment);
            if (activity is null)
                return NotFound(ApiErrorResponse.Create("Je bent niet ingeschreven voor deze activiteit."));
            return Ok(ApiSuccessResponse<ActivityDto>.Create(activity, "Je inschrijving is aangepast."));
        }

        /// <summary>
        /// Removes the authenticated user's registration, until the activity starts.
        /// </summary>
        [HttpDelete("{id:guid}/registration")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Unregister(Guid id)
        {
            if (await _activityService.GetActivityByIdAsync(id) is null)
                throw new EntityNotFoundException("Activity", id);
            if (!await _activityService.UnregisterAsync(id, CurrentUserId))
                return NotFound(ApiErrorResponse.Create("Je bent niet ingeschreven voor deze activiteit."));
            return NoContent();
        }

        private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);

        /// <summary>
        /// Gets activities by location.
        /// </summary>
        [HttpGet("search/location")]
        [ProducesResponseType(typeof(ApiSuccessResponse<IEnumerable<ActivityDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetActivitiesByLocation([FromQuery] string location)
        {
            if (string.IsNullOrWhiteSpace(location))
                throw new InvalidOperationApiException("Location parameter cannot be empty.");

            var activities = await _activityService.GetActivitiesByLocationAsync(location);
            return Ok(ApiSuccessResponse<IEnumerable<ActivityDto>>.Create(activities));
        }

        /// <summary>
        /// Gets activities with food.
        /// </summary>
        [HttpGet("search/with-food")]
        [ProducesResponseType(typeof(ApiSuccessResponse<IEnumerable<ActivityDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetActivitiesWithFood()
        {
            var activities = await _activityService.GetActivitiesWithFoodAsync();
            return Ok(ApiSuccessResponse<IEnumerable<ActivityDto>>.Create(activities));
        }

        /// <summary>
        /// Gets activities by price range.
        /// </summary>
        [HttpGet("search/price-range")]
        [ProducesResponseType(typeof(ApiSuccessResponse<IEnumerable<ActivityDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetActivitiesByPriceRange([FromQuery] decimal minPrice, [FromQuery] decimal maxPrice)
        {
            if (minPrice < 0 || maxPrice < 0 || minPrice > maxPrice)
                throw new InvalidOperationApiException("Invalid price range: minPrice and maxPrice must be non-negative and minPrice must be less than or equal to maxPrice.");

            var activities = await _activityService.GetActivitiesByPriceRangeAsync(minPrice, maxPrice);
            return Ok(ApiSuccessResponse<IEnumerable<ActivityDto>>.Create(activities));
        }

        /// <summary>
        /// Gets upcoming activities.
        /// </summary>
        [HttpGet("search/upcoming")]
        [ProducesResponseType(typeof(ApiSuccessResponse<IEnumerable<ActivityDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUpcomingActivities()
        {
            var activities = await _activityService.GetUpcomingActivitiesAsync();
            return Ok(ApiSuccessResponse<IEnumerable<ActivityDto>>.Create(activities));
        }

        /// <summary>
        /// Gets activities with available spots.
        /// </summary>
        [HttpGet("search/available-spots")]
        [ProducesResponseType(typeof(ApiSuccessResponse<IEnumerable<ActivityDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetActivitiesWithAvailableSpots()
        {
            var activities = await _activityService.GetActivitiesWithAvailableSpotsAsync();
            return Ok(ApiSuccessResponse<IEnumerable<ActivityDto>>.Create(activities));
        }
    }
}
