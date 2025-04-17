using Portfolio.Proxies.Dtos;

namespace Portfolio.Proxies.Impl;

public class AuthProxy : ProxyBase, IAuthProxy
{
    public AuthProxy(HttpClient httpClient) : base("AuthService", httpClient) { }

    public virtual Task<WalletId?> GetWalletIdAsync()
    {
        return GetAsync<WalletId>("user/wallet");
    }
}