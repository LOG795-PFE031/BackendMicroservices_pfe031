using Portfolio.Domain;

namespace Portfolio.Queries.Interfaces;

public interface IWalletQueryContext
{
    IQueryable<Wallet> Wallets { get; }
}