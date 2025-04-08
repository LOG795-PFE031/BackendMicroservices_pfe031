using MongoDB.Driver;
using Stock.Domain;
using Stock.Domain.Monads;
using Stock.Domain.ValueObjects;
using Stock.Queries.Seedwork;

namespace Stock.Queries.Quotes;

public sealed class GetQuotesHandler : IQueryHandler<GetQuotes, List<Quote>>
{
    private readonly IMongoCollection<Share> _shares;

    public GetQuotesHandler(IMongoClient client)
    {
        var database = client.GetDatabase("Stocks");
        _shares = database.GetCollection<Share>("Shares");
    }

    public async Task<Result<List<Quote>>> Handle(GetQuotes query, CancellationToken cancellation)
    {
        Share? share = await _shares.Find(s => s.Id == query.Symbol).FirstOrDefaultAsync(cancellationToken: cancellation);

        if (share is null)
        {
            return Result.Failure<List<Quote>>(new ArgumentException("Share not found"));
        }

        return Result.Success(share.Quotes.ToList());    
    }
}