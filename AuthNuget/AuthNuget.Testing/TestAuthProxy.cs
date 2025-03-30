using AuthNuget.Proxies;
using AuthNuget.Proxies.Impl;
using AuthNuget.Security;

namespace AuthNuget.Testing;

internal class TestAuthProxy : IAuthServiceProxy
{
    public Task<AuthServiceProxy.ServerPublicKey> GetPublicKey()
    {
        return Task.FromResult(new AuthServiceProxy.ServerPublicKey
        {
            PublicKey = RsaKeyStorage.Instance.PublicKey
        });
    }
}