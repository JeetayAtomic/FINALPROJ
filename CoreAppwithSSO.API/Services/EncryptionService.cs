using Microsoft.AspNetCore.DataProtection;

namespace CoreAppwithSSO.API.Services;

public interface IEncryptionService
{
    string EncryptApiKey(string plaintext);
    string DecryptApiKey(string ciphertext);
}

/// <summary>
/// Symmetric encryption for sensitive configuration backed by ASP.NET Core Data Protection.
/// Keys are persisted via the IDataProtectionKeyContext registered on the identity DB.
/// </summary>
public class EncryptionService : IEncryptionService
{
    private const string ApiKeyPurpose = "CoreAppwithSSO.ApiKey.Secret.v1";

    private readonly IDataProtector _apiKeyProtector;

    public EncryptionService(IDataProtectionProvider provider)
    {
        _apiKeyProtector = provider.CreateProtector(ApiKeyPurpose);
    }

    public string EncryptApiKey(string plaintext) =>
        string.IsNullOrEmpty(plaintext) ? string.Empty : _apiKeyProtector.Protect(plaintext);

    public string DecryptApiKey(string ciphertext) =>
        string.IsNullOrEmpty(ciphertext) ? string.Empty : _apiKeyProtector.Unprotect(ciphertext);
}
