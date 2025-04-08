using News.Queries.Seedwork;

namespace News.Queries.News;

public sealed record GetNews(string SymbolId) : IQuery;