using Portfolio.Domain.Monads;
using Portfolio.Proxies;
using Portfolio.Proxies.Dtos;
using Portfolio.Queries.Seedwork;

namespace Portfolio.Queries.User;

public sealed class GetUserWalletIdHandler : IQueryHandler<GetUserWalletId, string>
{
    private readonly IAuthProxy _client;

    public GetUserWalletIdHandler(IAuthProxy client)
    {
        _client = client;
    }

    public async Task<Result<string>> Handle(GetUserWalletId query, CancellationToken cancellation)
    {
        WalletId? walletId = await _client.GetWalletIdAsync();

        if (walletId is null)
        {
            return Result.Failure<string>("Wallet Id not found");
        }

        return Result.Success(walletId.Value);
    }
}