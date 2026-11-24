using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Shared.DTOs;

namespace UI.Service
{
    /// <summary>
    /// Gets activity data from the API over HTTP.
    /// The HttpClient base address is configured in Program.cs.
    /// Every call carries the JWT of the logged in visitor, because the API requires it.
    /// </summary>
    public class ActivityApiService : IActivityApiService
    {
        private readonly HttpClient _httpClient;
        private readonly JwtTokenStore _tokenStore;

        public ActivityApiService(HttpClient httpClient, JwtTokenStore tokenStore)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _tokenStore = tokenStore ?? throw new ArgumentNullException(nameof(tokenStore));
        }

        public async Task<IReadOnlyList<ActivityDto>> GetAllActivitiesAsync()
        {
            using var request = await CreateRequestAsync(HttpMethod.Get, "api/activities");
            using var response = await SendAsync(request);

            var result = await response.Content.ReadFromJsonAsync<ApiSuccessResponse<List<ActivityDto>>>();
            return result?.Data ?? new List<ActivityDto>();
        }

        public async Task<IReadOnlyList<ActivityDto>> GetMyActivitiesAsync()
        {
            using var request = await CreateRequestAsync(HttpMethod.Get, "api/activities/mine");
            using var response = await SendAsync(request);

            var result = await response.Content.ReadFromJsonAsync<ApiSuccessResponse<List<ActivityDto>>>();
            return result?.Data ?? new List<ActivityDto>();
        }

        public async Task<ActivityDto> CreateActivityAsync(ActivityInputDto newActivity)
        {
            using var request = await CreateRequestAsync(HttpMethod.Post, "api/activities");
            request.Content = JsonContent.Create(newActivity);
            using var response = await SendAsync(request);

            var created = await response.Content.ReadFromJsonAsync<ApiSuccessResponse<ActivityDto>>();
            return created?.Data
                ?? throw new HttpRequestException("The API did not return the created activity.");
        }

        public Task<ActivityDto> GetActivityAsync(Guid id) => ReadActivityAsync(HttpMethod.Get, $"api/activities/{id}");
        public Task<ActivityDto> UpdateActivityAsync(Guid id, ActivityInputDto activity) => ReadActivityAsync(HttpMethod.Put, $"api/activities/{id}", activity);
        public Task<ActivityDto> RegisterAsync(Guid id, bool bringsGuest = false, string? comment = null) => ReadActivityAsync(HttpMethod.Post, $"api/activities/{id}/registration", new RegisterActivityDto { BringsGuest = bringsGuest, Comment = comment });
        public Task<ActivityDto> UpdateRegistrationAsync(Guid id, bool bringsGuest, string? comment) => ReadActivityAsync(HttpMethod.Put, $"api/activities/{id}/registration", new RegisterActivityDto { BringsGuest = bringsGuest, Comment = comment });

        public async Task UnregisterAsync(Guid id)
        {
            using var request = await CreateRequestAsync(HttpMethod.Delete, $"api/activities/{id}/registration");
            using var response = await SendAsync(request);
        }

        public async Task<VisitorRegistrationResultDto> RegisterVisitorAsync(Guid id, VisitorRegistrationDto visitor)
        {
            using var request = await CreateRequestAsync(HttpMethod.Post, $"api/activities/{id}/visitor-registrations");
            request.Content = JsonContent.Create(visitor);
            using var response = await SendAsync(request);
            return (await response.Content.ReadFromJsonAsync<ApiSuccessResponse<VisitorRegistrationResultDto>>())?.Data
                ?? throw new HttpRequestException("Geen inschrijving ontvangen.");
        }

        public async Task<VisitorCancellationResultDto> CancelVisitorRegistrationAsync(CancelVisitorRegistrationDto cancellation)
        {
            using var request = await CreateRequestAsync(HttpMethod.Post, "api/visitor-registrations/cancel");
            request.Content = JsonContent.Create(cancellation);
            using var response = await SendAsync(request);
            return (await response.Content.ReadFromJsonAsync<ApiSuccessResponse<VisitorCancellationResultDto>>())?.Data
                ?? throw new HttpRequestException("Geen bevestiging ontvangen.");
        }

        private async Task<ActivityDto> ReadActivityAsync(HttpMethod method, string url, object? activity = null)
        {
            using var request = await CreateRequestAsync(method, url);
            if (activity is not null) request.Content = JsonContent.Create(activity);
            using var response = await SendAsync(request);
            return (await response.Content.ReadFromJsonAsync<ApiSuccessResponse<ActivityDto>>())?.Data
                ?? throw new HttpRequestException("Geen activiteit ontvangen.");
        }

        public async Task<IReadOnlyList<ActivityRegistrationDto>> GetActivityRegistrationsAsync(Guid id)
        {
            using var request = await CreateRequestAsync(HttpMethod.Get, $"api/activities/{id}/registrations");
            using var response = await SendAsync(request);
            var result = await response.Content.ReadFromJsonAsync<ApiSuccessResponse<List<ActivityRegistrationDto>>>();
            return result?.Data ?? new List<ActivityRegistrationDto>();
        }

        public async Task<RegistrationDto> GetRegistrationAsync(Guid id)
        {
            using var request = await CreateRequestAsync(HttpMethod.Get, $"api/activities/{id}/registration");
            using var response = await SendAsync(request);
            return (await response.Content.ReadFromJsonAsync<ApiSuccessResponse<RegistrationDto>>())?.Data ?? new RegistrationDto();
        }

        public async Task DeleteActivityAsync(Guid id)
        {
            using var request = await CreateRequestAsync(HttpMethod.Delete, $"api/activities/{id}");
            using var response = await SendAsync(request);
        }

        private async Task<HttpRequestMessage> CreateRequestAsync(HttpMethod method, string url)
        {
            var request = new HttpRequestMessage(method, url);

            var token = await _tokenStore.GetTokenAsync();
            if (!string.IsNullOrWhiteSpace(token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return request;
        }

        private async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            var response = await _httpClient.SendAsync(request);

            // No token, or an expired one: the visitor has to log in again.
            if (response.StatusCode is HttpStatusCode.Unauthorized)
            {
                response.Dispose();
                throw new NotLoggedInException();
            }

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
                // Validation errors say what is wrong ("Vul een geldig telefoonnummer in."), so show those when there are any.
                var message = error?.Errors?.Any() == true
                    ? string.Join(" ", error.Errors)
                    : error?.Message ?? "De aanvraag is mislukt.";
                var statusCode = response.StatusCode;
                response.Dispose();
                throw new HttpRequestException(message, null, statusCode);
            }
            return response;
        }
        public async Task<string> UploadImageAsync(
    Stream stream,
    string fileName,
    string contentType)
        {
            using var request =
                await CreateRequestAsync(HttpMethod.Post, "api/activities/upload-image");

            using var content = new MultipartFormDataContent();

            using var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType =
                new MediaTypeHeaderValue(contentType);

            content.Add(fileContent, "file", fileName);

            request.Content = content;

            using var response = await SendAsync(request);

            var result =
                await response.Content.ReadFromJsonAsync<ApiSuccessResponse<string>>();

            return result?.Data
                ?? throw new HttpRequestException(
                    "De API gaf geen afbeeldings-URL terug.");
        }
    }
}
