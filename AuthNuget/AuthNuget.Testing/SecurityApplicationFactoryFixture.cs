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

    public HttpClient WithAdminAuth() => WithJwt(RoleConstants.AdminRole, "testUser");

    public HttpClient WithAdminAuth(string username) => WithJwt(RoleConstants.AdminRole, username);

    public HttpClient WithClientAuth() => WithJwt(RoleConstants.Client, "testUser");

    public HttpClient WithClientAuth(string username) => WithJwt(RoleConstants.Client, username);

    private HttpClient WithJwt(string role, string username)
    {
        var httpClient = CreateDefaultClient();

        string jwt = JwtFactory.CreateJwtToken(username, role, RsaKeyStorage.Instance.RsaSecurityKey);

        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {jwt.Trim()}");

        return httpClient;
    }
}