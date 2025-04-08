namespace Portfolio.Proxies.Impl;

public abstract class ProxyBase
{
    private readonly string _baseUrl;
    private readonly HttpClient _httpClient;

    protected ProxyBase(string baseUrl, HttpClient httpClient)
    {
        _baseUrl = baseUrl;
        _httpClient = httpClient;
    }

    protected Task<T?> GetAsync<T>(string endpoint)
    {
        return _httpClient.GetFromJsonAsync<T>($"{_baseUrl}/{endpoint}");
    }
}