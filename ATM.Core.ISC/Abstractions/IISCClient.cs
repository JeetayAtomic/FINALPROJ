namespace ATM.Core.ISC.Abstractions;
using System.Threading.Tasks;

public interface IISCClient
{
    string TenantId { get; set; }
    Task<TResponse?> GetAsync<TResponse>(string relativeUrl, string? clientName = null);
    Task<TResponse?> PostAsync<TRequest, TResponse>(string relativeUrl, TRequest request, string? clientName = null);
    Task<TResponse?> PutAsync<TRequest, TResponse>(string relativeUrl, TRequest request, string? clientName = null);
    Task<bool> DeleteAsync(string relativeUrl, string? clientName = null);
    Task<TResponse?> PatchAsync<TRequest, TResponse>(string relativeUrl, TRequest request, string? clientName = null);
}
