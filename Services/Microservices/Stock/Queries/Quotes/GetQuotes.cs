using Stock.Queries.Seedwork;

namespace Stock.Queries.Quotes;

public record GetQuotes(string Symbol) : IQuery;