using System.Net.Http.Json;
using Shared.DTOs;

namespace UI.Service;

public sealed class AuthenticationApiService(HttpClient httpClient) : IAuthenticationApiService
{
    public async Task<LoginResult> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync("api/auth/login", request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadFromJsonAsync<ApiSuccessResponse<LoginResponse>>(
                cancellationToken: cancellationToken);

            return body?.Data is { AccessToken.Length: > 0 } user
                ? LoginResult.Success(user)
                : LoginResult.Failure("De API gaf geen geldig toegangstoken terug.");
        }

        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(
            cancellationToken: cancellationToken);

        return LoginResult.Failure(
            error?.Message ?? "Inloggen is mislukt. Controleer je gegevens en probeer het opnieuw.");
    }
}
