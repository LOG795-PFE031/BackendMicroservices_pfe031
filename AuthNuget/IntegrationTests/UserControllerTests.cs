using System.Net;
using FluentAssertions;

namespace IntegrationTests;

[Collection(nameof(TestCollections.Default))]
public class UserControllerTests
{
    private readonly ApplicationFactoryFixture _applicationFactoryFixture;

    public UserControllerTests(ApplicationFactoryFixture applicationFactoryFixture)
    {
        _applicationFactoryFixture = applicationFactoryFixture;
    }

    [Fact]
    public async Task ValidAuth_Secure_ShouldReturnOK()
    {
        var client = _applicationFactoryFixture.WithAdminAuth();

        var response = await client.GetAsync("secure");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Client_Secure_ShouldReturnForbidden()
    {
        var client = _applicationFactoryFixture.WithClientAuth();

        var response = await client.GetAsync("secure");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task NoAuth_Secure_ShouldReturnUnauthorized()
    {
        var client = _applicationFactoryFixture.CreateClient();

        var response = await client.GetAsync("secure");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}