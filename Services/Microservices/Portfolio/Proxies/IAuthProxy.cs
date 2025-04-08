using Portfolio.Proxies.Dtos;

namespace Portfolio.Proxies;

public interface IAuthProxy
{
    Task<WalletId?> GetWalletIdAsync();
}