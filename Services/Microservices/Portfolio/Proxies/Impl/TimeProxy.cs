using Portfolio.Proxies.Dtos;

namespace Portfolio.Proxies.Impl;

public sealed class TimeProxy : ProxyBase, ITimeProxy
{
    public TimeProxy(HttpClient httpClient) : base("TimeService", httpClient) { }

    public Task<CurrentTime?> GetCurrentTime()
    {
        return GetAsync<CurrentTime>("time");
    }
}