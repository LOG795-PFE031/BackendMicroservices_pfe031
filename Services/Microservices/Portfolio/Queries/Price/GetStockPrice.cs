using Portfolio.Queries.Seedwork;

namespace Portfolio.Queries.Price;

public record GetStockPrice(string Symbol, DateTime DateTime) : IQuery;