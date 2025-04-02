using System.Net;
using System.Net.Http.Json;
using AuthNuget.Security;
using AuthService.Commands.NewUser;
using AuthService.Configurations;
using AuthService.Controllers;
using AuthService.Dtos;
using AuthServiceIntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace AuthServiceIntegrationTests.Tests.Auth;

[Collection(nameof(TestCollections.Default))]
public class UserControllerTests
{
    private readonly ApplicationFactoryFixture _applicationFactoryFixture;

    public UserControllerTests(ApplicationFactoryFixture applicationFactoryFixture)
    {
        _applicationFactoryFixture = applicationFactoryFixture;
    }

    [Fact]
    public async Task ValidAuth_Validate_ShouldReturnOK()
    {
        var client = _applicationFactoryFixture.WithAdminAuth();

        var response = await client.GetAsync("user/validate");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ValidAuth_ChangePassword_ShouldReturnOK()
    {
        var defaultAdmin = _applicationFactoryFixture.Services.GetRequiredService<IOptions<DefaultAdmin>>();

        var client = _applicationFactoryFixture.WithAdminAuth(defaultAdmin.Value.Username);

        var passwordChangeDto = new PasswordChangeDto("newSecret", "secret");

        var response = await client.PatchAsJsonAsync("user/password", passwordChangeDto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify new login
        var httpResponseMessage = await _applicationFactoryFixture.SigninAsync(new UserCredentials()
        {
            Username = defaultAdmin.Value.Username,
            Password = passwordChangeDto.NewPassword
        });

        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);

        var jwt = await httpResponseMessage.Content.ReadAsStringAsync();

        jwt.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ValidAuth_ChangePasswordShortPassword_ShouldReturnBadRequest()
    {
        var client = _applicationFactoryFixture.WithAdminAuth();

        var passwordChangeDto = new PasswordChangeDto("nw", "secret");

        var response = await client.PatchAsJsonAsync("user/password", passwordChangeDto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task InValidAuth_ChangePassword_ShouldReturnForbidden()
    {
        var client = _applicationFactoryFixture.CreateDefaultClient();

        var passwordChangeDto = new PasswordChangeDto(string.Empty, string.Empty);

        var response = await client.PatchAsJsonAsync("user/password", JsonConvert.SerializeObject(passwordChangeDto));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ValidAuth_CreateUser_ShouldReturnOK()
    {
        var client = _applicationFactoryFixture.WithAdminAuth();

        var createUser = new CreateUser("newUser", "secret", RoleConstants.Client);

        var response = await client.PostAsJsonAsync("users", createUser);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseMessage = await _applicationFactoryFixture.SigninAsync(new UserCredentials()
        {
            Username = "newUser",
            Password = "secret"
        });

        responseMessage.StatusCode.Should().Be(HttpStatusCode.OK);

        var jwt = await responseMessage.Content.ReadAsStringAsync();

        jwt.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ValidAuth_GetWalletId_ShouldReturnOk()
    {
        var defaultAdmin = _applicationFactoryFixture.Services.GetRequiredService<IOptions<DefaultAdmin>>();

        var client = _applicationFactoryFixture.WithAdminAuth(defaultAdmin.Value.Username);

        UserController.WalletId? response = await client.GetFromJsonAsync<UserController.WalletId>("user/wallet");

        response.Should().NotBeNull();

        response.Value.Should().HaveLength(Guid.NewGuid().ToString().Length);
    }
}