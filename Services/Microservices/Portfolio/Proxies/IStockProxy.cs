using Portfolio.Proxies.Dtos;

namespace Portfolio.Proxies;

public interface IStockProxy
{
    Task<StockPrice?> GetStockPrice(string symbol, DateTime date);
}