using Shared.DTOs;

namespace UI.Service;

public sealed record LoginResult(LoginResponse? User, string? ErrorMessage)
{
    public bool Succeeded => User is not null;

    public static LoginResult Success(LoginResponse user) => new(user, null);
    public static LoginResult Failure(string message) => new(null, message);
}
