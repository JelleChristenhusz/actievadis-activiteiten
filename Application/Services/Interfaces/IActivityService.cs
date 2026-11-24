using System.Collections.Generic;
using System.Threading.Tasks;
using Shared.DTOs;

namespace Application.Services.Interfaces
{
    /// <summary>
    /// Service interface for Activity business logic.
    /// </summary>
    public interface IActivityService
    {
        /// <summary>
        /// Gets all activities.
        /// </summary>
        Task<IEnumerable<ActivityDto>> GetAllActivitiesAsync();

        /// <summary>
        /// Gets the activities a normal user may see: not deleted and not yet finished, soonest first.
        /// </summary>
        Task<IEnumerable<ActivityDto>> GetVisibleActivitiesAsync();

        /// <summary>
        /// Gets every activity for administrators, including finished ones, sorted by start time. Deleted ones are never returned.
        /// </summary>
        Task<IEnumerable<ActivityDto>> GetAllActivitiesForAdministratorAsync();

        /// <summary>
        /// Gets an activity by ID.
        /// </summary>
        Task<ActivityDto?> GetActivityByIdAsync(Guid id);

        /// <summary>
        /// Creates a new activity.
        /// </summary>
        Task<ActivityDto> CreateActivityAsync(ActivityInputDto createDto);

        /// <summary>
        /// Updates an existing activity.
        /// </summary>
        Task<ActivityDto?> UpdateActivityAsync(Guid id, ActivityInputDto updateDto);

        /// <summary>
        /// Deletes an activity. It is kept in the database (marked as deleted), but nobody sees it anymore.
        /// </summary>
        Task<bool> DeleteActivityAsync(Guid id);
        Task<bool> IsRegisteredAsync(Guid id, Guid userId);
        Task<RegistrationDto> GetRegistrationAsync(Guid id, Guid userId);
        Task<IEnumerable<ActivityRegistrationDto>> GetActivityRegistrationsAsync(Guid activityId);
        Task<ActivityDto?> RegisterAsync(Guid id, Guid userId, bool bringsGuest = false, string? comment = null);

        /// <summary>
        /// Changes the +1 and the comment of an existing registration, as long as the activity has not started.
        /// Returns null when the user is not registered for the activity.
        /// </summary>
        Task<ActivityDto?> UpdateRegistrationAsync(Guid id, Guid userId, bool bringsGuest, string? comment);

        /// <summary>
        /// Removes the user's registration, as long as the activity has not started. Returns false when the user was not registered.
        /// </summary>
        Task<bool> UnregisterAsync(Guid id, Guid userId);

        /// <summary>
        /// Registers a visitor without an account (a "gast"). The visitor counts towards the maximum and may bring a +1
        /// when the activity allows it. Returns null when the activity does not exist. The result contains the code
        /// the visitor needs to unregister; only a hash of it is stored.
        /// </summary>
        Task<VisitorRegistrationResultDto?> RegisterVisitorAsync(Guid id, VisitorRegistrationDto visitor);

        /// <summary>
        /// Cancels a visitor registration by e-mail address and cancellation code, as long as the activity has not started.
        /// Returns null when no registration matches.
        /// </summary>
        Task<VisitorCancellationResultDto?> CancelVisitorRegistrationAsync(string email, string code);

        /// <summary>
        /// Gets activities by location.
        /// </summary>
        Task<IEnumerable<ActivityDto>> GetActivitiesByLocationAsync(string location);

        /// <summary>
        /// Gets activities with food.
        /// </summary>
        Task<IEnumerable<ActivityDto>> GetActivitiesWithFoodAsync();

        /// <summary>
        /// Gets activities within a cost range.
        /// </summary>
        Task<IEnumerable<ActivityDto>> GetActivitiesByPriceRangeAsync(decimal minPrice, decimal maxPrice);

        /// <summary>
        /// Gets upcoming activities.
        /// </summary>
        Task<IEnumerable<ActivityDto>> GetUpcomingActivitiesAsync();

        /// <summary>
        /// Gets activities with available spots.
        /// </summary>
        Task<IEnumerable<ActivityDto>> GetActivitiesWithAvailableSpotsAsync();
        Task<IEnumerable<ActivityDto>> GetActivitiesForUserAsync(Guid userId);
    }
}
