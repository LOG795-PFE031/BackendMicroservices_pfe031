using Portfolio.Proxies.Dtos;

namespace Portfolio.Proxies.Impl;

public class TimeProxy : ProxyBase, ITimeProxy
{
    public TimeProxy(HttpClient httpClient) : base("TimeService", httpClient) { }

    public virtual Task<CurrentTime?> GetCurrentTime()
    {
        return GetAsync<CurrentTime>("time");
    }
}