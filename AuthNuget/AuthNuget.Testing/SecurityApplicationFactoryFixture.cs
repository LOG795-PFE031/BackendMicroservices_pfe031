using AuthNuget.Registration;
using AuthNuget.Security;
using Microsoft.AspNetCore.Mvc.Testing;

namespace AuthNuget.Testing;

public abstract class SecurityApplicationFactoryFixture<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
{
    protected SecurityApplicationFactoryFixture() 
    {
        PfeSecureHost.AuthServiceProxyFactory = (_, _) => new TestAuthProxy();
    }

    public HttpClient WithAdminAuth() => WithJwt(RoleConstants.AdminRole);

    public HttpClient WithClientAuth() => WithJwt(RoleConstants.Client);

    private HttpClient WithJwt(string role)
    {
        var httpClient = CreateDefaultClient();

        string jwt = JwtFactory.CreateJwtToken("testUser", role, RsaKeyStorage.Instance.RsaSecurityKey);

        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {jwt.Trim()}");

        return httpClient;
    }
}