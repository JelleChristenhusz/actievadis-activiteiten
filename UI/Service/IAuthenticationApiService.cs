using Shared.DTOs;

namespace UI.Service;

public interface IAuthenticationApiService
{
    Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
