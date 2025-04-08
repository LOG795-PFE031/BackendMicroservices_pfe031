using Portfolio.Proxies.Dtos;

namespace Portfolio.Proxies.Impl;

public sealed class AuthProxy : ProxyBase, IAuthProxy
{
    public AuthProxy(HttpClient httpClient) : base("AuthService", httpClient) { }

    public Task<WalletId?> GetWalletIdAsync()
    {
        return GetAsync<WalletId>("user/wallet");
    }
}