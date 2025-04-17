using Portfolio.Proxies.Dtos;

namespace Portfolio.Proxies.Impl;

public class StockProxy : ProxyBase, IStockProxy
{
    public StockProxy(HttpClient httpClient) : base("StockService", httpClient) { }

    public virtual Task<StockPrice?> GetStockPrice(string symbol, DateTime date)
    {
        return GetAsync<StockPrice>($"stocks/{symbol}/{date}");
    }
}