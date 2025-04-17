using Portfolio.Domain.Monads;
using Portfolio.Proxies;
using Portfolio.Proxies.Dtos;
using Portfolio.Queries.Seedwork;

namespace Portfolio.Queries.Price;

public sealed class GetStockPriceHandler : IQueryHandler<GetStockPrice, decimal>
{
    private readonly IStockProxy _client;

    public GetStockPriceHandler(IStockProxy client)
    {
        _client = client;
    }

    public async Task<Result<decimal>> Handle(GetStockPrice query, CancellationToken cancellation)
    {
        StockPrice? share = await _client.GetStockPrice(query.Symbol, query.DateTime);

        if (share is null)
        {
            return Result.Failure<decimal>($"Share with symbol '{query.Symbol}' not found");
        }

        return Result.Success(share.Value);
    }
}