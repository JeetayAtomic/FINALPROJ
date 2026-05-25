using System.Security.Cryptography;
using System.Text;

namespace CoreAppwithSSO.API.Services;

public interface IApiKeyService
{
    /// <summary>Generates a fresh (prefix, fullKey, hash) tuple. The full key is shown to the caller exactly once.</summary>
    (string Prefix, string FullKey, string Hash) Generate();

    /// <summary>Extracts the lookup prefix from a presented key. Returns null if malformed.</summary>
    string? ExtractPrefix(string fullKey);

    /// <summary>Verifies a presented key against a stored hash in constant time.</summary>
    bool Verify(string presented, string storedHash);
}

public class ApiKeyService : IApiKeyService
{
    public const string Prefix = "ad_";
    private const int PrefixLookupLength = 8;
    private const int SecretBytes = 32;

    public (string Prefix, string FullKey, string Hash) Generate()
    {
        var raw = RandomNumberGenerator.GetBytes(SecretBytes);
        // url-safe base64 without padding for a clean token
        var secret = Convert.ToBase64String(raw)
            .Replace('+', '-').Replace('/', '_').TrimEnd('=');

        var full = Prefix + secret;
        var lookup = secret[..PrefixLookupLength];
        var hash = Hash(full);
        return (lookup, full, hash);
    }

    public string? ExtractPrefix(string fullKey)
    {
        if (string.IsNullOrEmpty(fullKey) || !fullKey.StartsWith(Prefix)) return null;
        var secret = fullKey[Prefix.Length..];
        return secret.Length < PrefixLookupLength ? null : secret[..PrefixLookupLength];
    }

    public bool Verify(string presented, string storedHash)
    {
        var actual = Hash(presented);
        return CryptographicOperations.FixedTimeEquals(
            Encoding.ASCII.GetBytes(actual),
            Encoding.ASCII.GetBytes(storedHash));
    }

    private static string Hash(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
