using MongoDB.Driver;
using Stock.Domain;
using Stock.Domain.Monads;
using Stock.Queries.Seedwork;

namespace Stock.Queries.Stocks;

public sealed class GetAllSymbolsHandler : IQueryHandler<GetAllSymbols, List<string>>
{
    private readonly IMongoCollection<Share> _shares;

    public GetAllSymbolsHandler(IMongoClient client)
    {
        var database = client.GetDatabase("Stocks");
        _shares = database.GetCollection<Share>("Shares");
    }

    public async Task<Result<List<string>>> Handle(GetAllSymbols query, CancellationToken cancellation)
    {
        var symbols = await _shares.DistinctAsync(share => share.Symbol, share => true, cancellationToken: cancellation);

        return Result.Success(await symbols.ToListAsync(cancellationToken: cancellation));
    }
}