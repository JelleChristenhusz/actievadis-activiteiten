using Application.DTOs;

namespace API.Authentication;

/// <summary>Issues signed access tokens for authenticated users.</summary>
public interface IJwtTokenService
{
    /// <summary>Issues a signed token for the supplied verified identity.</summary>
    TokenResult Create(AuthenticatedUserDto user);
}
