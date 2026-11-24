namespace API.Authentication;

/// <summary>Contains an issued access token and its UTC expiration time.</summary>
public sealed record TokenResult(string AccessToken, DateTime ExpiresAtUtc);
