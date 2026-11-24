using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Application.Security;

/// <summary>Hashes passwords using ASP.NET Core Identity and upgrades the previous hash format.</summary>
public static class PasswordHasher
{
    private static readonly object HashContext = new();
    private static readonly PasswordHasher<object> Hasher = new(Options.Create(new PasswordHasherOptions
    {
        CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3,
        IterationCount = 210_000
    }));

    /// <summary>Creates a salted, versioned Identity password hash.</summary>
    public static string Hash(string password) => Hasher.HashPassword(HashContext, password);

    /// <summary>Verifies a password and indicates whether its stored hash must be upgraded.</summary>
    public static bool Verify(string password, string passwordHash, out bool needsRehash)
    {
        needsRehash = false;
        if (string.IsNullOrEmpty(passwordHash) || passwordHash.Length > 512)
            return false;

        try
        {
            // Read-only compatibility with hashes produced by the previous application.
            // Never create a new hash in this format.
            if (passwordHash.Contains('.'))
            {
                var parts = passwordHash.Split('.');
                if (parts.Length != 3 || parts[0] != "100000")
                    return false;

                var salt = Convert.FromBase64String(parts[1]);
                var expectedHash = Convert.FromBase64String(parts[2]);
                if (salt.Length != 16 || expectedHash.Length != 32)
                    return false;

                var actualHash = Rfc2898DeriveBytes.Pbkdf2(
                    password, salt, 100_000, HashAlgorithmName.SHA256, 32);
                needsRehash = CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
                return needsRehash;
            }

            var result = Hasher.VerifyHashedPassword(HashContext, passwordHash, password);
            needsRehash = result == PasswordVerificationResult.SuccessRehashNeeded;
            return result != PasswordVerificationResult.Failed;
        }
        catch (Exception exception) when (exception is FormatException or ArgumentException or CryptographicException)
        {
            return false;
        }
    }
}
