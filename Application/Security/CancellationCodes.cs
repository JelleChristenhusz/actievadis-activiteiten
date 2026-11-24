using System.Security.Cryptography;

namespace Application.Security;

/// <summary>
/// Creates and checks the codes visitors without an account use to unregister, for example "K7MQ-2XPA".
/// The code is only shown (and e-mailed) once; the database stores a hash, like a password.
/// </summary>
public static class CancellationCodes
{
    // No 0/O and 1/I/L, so the code is easy to read and type over.
    private const string Alphabet = "ABCDEFGHJKMNPQRSTUVWXYZ23456789";
    private const int Length = 8;

    /// <summary>Creates a new random code, written as two groups of four characters.</summary>
    public static string Create()
    {
        var code = RandomNumberGenerator.GetString(Alphabet, Length);
        return $"{code[..4]}-{code[4..]}";
    }

    /// <summary>Hashes a code for storage.</summary>
    public static string Hash(string code) => PasswordHasher.Hash(Normalize(code));

    /// <summary>Checks a typed code against the stored hash. Case, spaces and dashes do not matter.</summary>
    public static bool Verify(string code, string? hash) =>
        hash is not null && PasswordHasher.Verify(Normalize(code), hash, out _);

    private static string Normalize(string code) =>
        new(code.Where(char.IsLetterOrDigit).Select(char.ToUpperInvariant).ToArray());
}
