using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace AuthNuget.Proxies.Impl;

public sealed class AuthServiceProxy : IAuthServiceProxy
{
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;
    private readonly AsyncRetryPolicy _retryPolicy;

    internal AuthServiceProxy(Uri authServerBaseUrl, ILogger logger)
    {
        _logger = logger;
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = authServerBaseUrl;

        _retryPolicy = Policy
            .Handle<HttpRequestException>()
            .WaitAndRetryForeverAsync(retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }

    public async Task<ServerPublicKey> GetPublicKey()
    {
        try
        {
            ServerPublicKey? publicKey = await _retryPolicy.ExecuteAsync(() => _httpClient.GetFromJsonAsync<ServerPublicKey>("publickey"));

            if (publicKey == null)
            {
                _logger.LogError("Failed to get public key from auth service");

                throw new Exception("Failed to get public key from auth service");
            }

            return publicKey;
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to get public key from auth service");
            throw;
        }
    }

    public sealed class ServerPublicKey
    {
        public required string PublicKey { get; set; }
    }
}