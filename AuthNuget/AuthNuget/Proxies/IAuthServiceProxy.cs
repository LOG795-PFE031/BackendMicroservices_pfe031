using AuthNuget.Proxies.Impl;

namespace AuthNuget.Proxies;

public interface IAuthServiceProxy
{
    Task<AuthServiceProxy.ServerPublicKey> GetPublicKey();
}