using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Security.Cryptography;
using System.Text.Json;
using Shared.DTOs;

namespace UI.Service
{
    /// <summary>
    /// Reads the JWT from the protected authentication session saved by the login page.
    /// Only works from an interactive component; during static rendering there is no JavaScript yet.
    /// </summary>
    public sealed class JwtTokenStore
    {
        private readonly ProtectedLocalStorage _storage;

        public JwtTokenStore(ProtectedLocalStorage storage)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        /// <summary>
        /// The logged in visitor, or null when nobody is logged in or the session has expired.
        /// </summary>
        public async Task<LoginResponse?> GetSessionAsync()
        {
            ProtectedBrowserStorageResult<LoginResponse> session;
            try
            {
                session = await _storage.GetAsync<LoginResponse>(AuthenticationStorage.SessionKey);
            }
            catch (Exception exception) when (exception is JsonException or CryptographicException)
            {
                // Old integer-ID sessions and expired encryption keys require a new login.
                await ClearTokenAsync();
                return null;
            }

            if (!session.Success || session.Value is null)
                return null;

            if (session.Value.ExpiresAtUtc <= DateTime.UtcNow)
            {
                await ClearTokenAsync();
                return null;
            }

            return session.Value;
        }

        public async Task<string?> GetTokenAsync() => (await GetSessionAsync())?.AccessToken;

        public async Task<bool> IsAdministratorAsync() => (await GetSessionAsync())?.IsAdministrator() == true;

        public async Task ClearTokenAsync()
        {
            await _storage.DeleteAsync(AuthenticationStorage.SessionKey);
        }
    }
}
