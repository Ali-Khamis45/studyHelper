using System.Security.Cryptography;

namespace AiStudyOS.Application.Identity.Services;

/// <summary>
/// Pure BCL crypto (RandomNumberGenerator/SHA256) with no external dependency, so it lives directly
/// in Application rather than behind an Infrastructure abstraction.
/// </summary>
public static class RefreshTokenGenerator
{
    public static string GenerateRawToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

    public static string Hash(string rawToken) => Convert.ToHexStringLower(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(rawToken)));
}
