using MongoDB.Driver;
using News.Domain;
using News.Domain.Monads;
using News.Queries.Seedwork;

namespace News.Queries.News;

public sealed class GetNewsHandler : IQueryHandler<GetNews, NewsViewModel>
{
    private readonly IMongoCollection<Article> _articles;

    public GetNewsHandler(IMongoClient client)
    {
        var database = client.GetDatabase("News");
        _articles = database.GetCollection<Article>("Articles");
    }

    public async Task<Result<NewsViewModel>> Handle(GetNews query, CancellationToken cancellation)
    {
        List<Article> articles = await _articles.Find(a => a.SymbolId == query.SymbolId).ToListAsync(cancellation);

        return Result.Success(new NewsViewModel(articles));
    }
}