using System.Net;
using System.Net.Http.Json;
using AuthNuget.Security;
using AuthService.Dtos;
using AuthService.HostedServices;
using AuthServiceIntegrationTests.Infrastructure;
using FluentAssertions;

namespace AuthServiceIntegrationTests.Tests.Auth;

[Collection(nameof(TestCollections.Default))]
public class TestAuthentification
{
    private readonly ApplicationFactoryFixture _applicationFactoryFixture;

    public TestAuthentification(ApplicationFactoryFixture applicationFactoryFixture)
    {
        _applicationFactoryFixture = applicationFactoryFixture;
    }

    [Theory]
    [InlineData("John", "secret", RoleConstants.Client)]
    public async Task AuthenticatedUser_ShouldReceiveOK(string username, string password, string role)
    {
        var unauthorizedClient = _applicationFactoryFixture.CreateDefaultClient();

        await ServiceReady.Instance.IsReady.Task;

        var userCredentials = new UserCredentials
        {
            Username = username,
            Password = password
        };

        // Post encrypted credentials to the server
        var response = await unauthorizedClient.PostAsJsonAsync("auth/signin", userCredentials);

        // Check the response status code
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        string jwt = await response.Content.ReadAsStringAsync();

        jwt.Should().NotBeNullOrEmpty();
    }
}