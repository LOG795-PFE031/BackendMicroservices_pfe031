using Stock.Queries.Seedwork;

namespace Stock.Queries.Price;

public record GetStockPrice(string Symbol, DateTime DateTime) : IQuery;