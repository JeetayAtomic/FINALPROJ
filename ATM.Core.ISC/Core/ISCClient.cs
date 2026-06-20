using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;

namespace ATM.Core.ISC.Core;

using ATM.Core.ISC.Abstractions;

public class ISCClient(IHttpClientFactory httpClientFactory, ILogger<ISCClient> logger, string defaultClientName = "DefaultClient") : IISCClient
{
    public string TenantId { get; set; } = "";

    #region Public Methods
    public async Task<TResponse?> GetAsync<TResponse>(string relativeUrl, string? clientName = null)
    {
        var client = GetClient(clientName);
        var requestMessage = new HttpRequestMessage(HttpMethod.Get, relativeUrl);
        requestMessage.Headers.Add("X-Tenant-Id", TenantId);
        var resp = await client.SendAsync(requestMessage);
        return await Deserialize<TResponse>(resp);
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string relativeUrl, TRequest request, string? clientName = null)
    {
        var client = GetClient(clientName);
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, relativeUrl)
        {
            Content = ToJsonContent(request)
        };
        requestMessage.Headers.Add("X-Tenant-Id", TenantId);
        var resp = await client.SendAsync(requestMessage);
        return await Deserialize<TResponse>(resp);
    }

    public async Task<TResponse?> PutAsync<TRequest, TResponse>(string relativeUrl, TRequest request, string? clientName = null)
    {
        var client = GetClient(clientName);
        var requestMessage = new HttpRequestMessage(HttpMethod.Put, relativeUrl)
        {
            Content = ToJsonContent(request)
        };
        requestMessage.Headers.Add("X-Tenant-Id", TenantId);
        var resp = await client.SendAsync(requestMessage);
        return await Deserialize<TResponse>(resp);
    }

    public async Task<bool> DeleteAsync(string relativeUrl, string? clientName = null )
    {
        var client = GetClient(clientName);
        var requestMessage = new HttpRequestMessage(HttpMethod.Delete, relativeUrl);
        requestMessage.Headers.Add("X-Tenant-Id", TenantId);
        var resp = await client.SendAsync(requestMessage);
        return resp.IsSuccessStatusCode;
    }

    public async Task<TResponse?> PatchAsync<TRequest, TResponse>(string relativeUrl, TRequest request, string? clientName = null)
    {
        var client = GetClient(clientName);
        var method = new HttpMethod("PATCH");
        var req = new HttpRequestMessage(method, relativeUrl)
        {
            Content = ToJsonContent(request)
        };
        req.Headers.Add("X-Tenant-Id", TenantId);
        var resp = await client.SendAsync(req);
        return await Deserialize<TResponse>(resp);
    }
    #endregion

    #region Private Methods
    private HttpClient GetClient(string? clientName) =>
      httpClientFactory.CreateClient(clientName ?? defaultClientName);

    private StringContent ToJsonContent<T>(T obj) =>
        new(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json");

    //private async Task<TResponse?> Deserialize<TResponse>(HttpResponseMessage resp)
    //{
    //    if (!resp.IsSuccessStatusCode)
    //    {
    //        logger.LogWarning("HTTP call returned {StatusCode} for {RequestUri}", resp.StatusCode, resp.RequestMessage?.RequestUri);
    //        return default;
    //    }

    //    var json = await resp.Content.ReadAsStringAsync();
    //    return JsonConvert.DeserializeObject<TResponse>(json);
    //}
    private async Task<TResponse?> Deserialize<TResponse>(HttpResponseMessage resp)
    {
        var json = await resp.Content.ReadAsStringAsync();
        if (!resp.IsSuccessStatusCode)
        {
            logger.LogWarning(
                "HTTP call returned {StatusCode} for {RequestUri}. Response: {ResponseBody}",
                resp.StatusCode,
                resp.RequestMessage?.RequestUri,
                json
            );
        }
        if (string.IsNullOrWhiteSpace(json))
            return default;
        return JsonConvert.DeserializeObject<TResponse>(json);
    }


    #endregion
}
