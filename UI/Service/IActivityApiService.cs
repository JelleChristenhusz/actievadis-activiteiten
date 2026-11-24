using Shared.DTOs;

namespace UI.Service
{
    /// <summary>
    /// Gets activity data from the API.
    /// Every method throws HttpRequestException when the API cannot be reached or returns an error,
    /// and NotLoggedInException when the visitor has to log in (again).
    /// </summary>
    public interface IActivityApiService
    {
        /// <summary>
        /// Gets the activities, sorted by start time. Normal users only get current activities;
        /// administrators also get finished ones. Deleted activities are never returned.
        /// </summary>
        Task<IReadOnlyList<ActivityDto>> GetAllActivitiesAsync();

        /// <summary>Gets the activities the logged-in visitor is registered for.</summary>
        Task<IReadOnlyList<ActivityDto>> GetMyActivitiesAsync();

        Task<ActivityDto> GetActivityAsync(Guid id);
        Task<IReadOnlyList<ActivityRegistrationDto>> GetActivityRegistrationsAsync(Guid id);
        Task<ActivityDto> CreateActivityAsync(ActivityInputDto newActivity);
        Task<ActivityDto> UpdateActivityAsync(Guid id, ActivityInputDto activity);
        Task DeleteActivityAsync(Guid id);
        Task<RegistrationDto> GetRegistrationAsync(Guid id);
        Task<ActivityDto> RegisterAsync(Guid id, bool bringsGuest = false, string? comment = null);

        /// <summary>Changes the +1 and the comment of the visitor's own registration.</summary>
        Task<ActivityDto> UpdateRegistrationAsync(Guid id, bool bringsGuest, string? comment);

        Task UnregisterAsync(Guid id);

        /// <summary>Registers a visitor without an account (a "gast"); works without logging in.</summary>
        Task<VisitorRegistrationResultDto> RegisterVisitorAsync(Guid id, VisitorRegistrationDto visitor);

        /// <summary>Cancels a visitor registration with e-mail address and cancellation code; works without logging in.</summary>
        Task<VisitorCancellationResultDto> CancelVisitorRegistrationAsync(CancelVisitorRegistrationDto request);
        Task<string> UploadImageAsync(Stream stream, string fileName, string contentType);
    }
}
