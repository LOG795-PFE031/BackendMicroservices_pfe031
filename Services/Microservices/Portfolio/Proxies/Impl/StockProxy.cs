using Portfolio.Proxies.Dtos;

namespace Portfolio.Proxies.Impl;

public sealed class StockProxy : ProxyBase, IStockProxy
{
    public StockProxy(HttpClient httpClient) : base("StockService", httpClient) { }

    public Task<StockPrice?> GetStockPrice(string symbol, DateTime date)
    {
        return GetAsync<StockPrice>($"stocks/{symbol}/{date}");
    }
}