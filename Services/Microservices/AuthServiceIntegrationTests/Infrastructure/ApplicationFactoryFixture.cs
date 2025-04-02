using AuthNuget.Testing;
using AuthService;
using AuthService.Commands.NewUser;
using AuthService.Dtos;
using AuthServiceIntegrationTests.Infrastructure.TestContainer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RabbitMqNuget.Registration;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;

namespace AuthServiceIntegrationTests.Infrastructure;

public sealed class ApplicationFactoryFixture : SecurityApplicationFactoryFixture<Startup>, IAsyncLifetime
{
    private readonly Postgres _postgres = new();
    private readonly Rabbitmq _rabbitmq = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureAppConfiguration((_, config) =>
        {
            var integrationConfig = new Dictionary<string, string>
            {
                ["ConnectionStrings:Postgres"] = _postgres.Container.GetConnectionString(),
                ["ConnectionStrings:Rabbitmq"] = _rabbitmq.Container.GetConnectionString(),
            };
            
            config.AddInMemoryCollection(integrationConfig!);
        });

        builder.ConfigureTestServices(services =>
        {
            // Remove any existing MassTransit registrations to prevent duplicate configuration.
            var massTransitDescriptors = services
                .Where(s => s.ServiceType?.Namespace?.Contains("MassTransit") == true)
                .ToList();

            foreach (var descriptor in massTransitDescriptors)
            {
                services.Remove(descriptor);
            }

            services.RegisterMassTransit(
                _rabbitmq.Container.GetConnectionString(),
                new MassTransitConfigurator()
                    .AddPublisher<UserCreated>("user-created-exchange"));
        });
    }

    public async Task<HttpResponseMessage> SigninAsync(UserCredentials userCredentials)
    {
        var unauthorizedClient = CreateDefaultClient();

        // Fetch public key from the server
        var publicKeyResponse = await unauthorizedClient.GetFromJsonAsync<ServerPublicKey>("auth/publickey");

        string pemPublicKey = publicKeyResponse.PublicKey;

        using var rsa = RSA.Create();

        rsa.ImportFromPem(pemPublicKey.ToCharArray());  // Import public key

        // Serialize and encrypt the credentials
        string serializedCredentials = JsonConvert.SerializeObject(userCredentials);
        byte[] credentialsBytes = Encoding.UTF8.GetBytes(serializedCredentials);
        byte[] encryptedData = rsa.Encrypt(credentialsBytes, RSAEncryptionPadding.OaepSHA256);

        var encryptedCredentials = new EncryptedCredentials
        {
            EncryptedData = Convert.ToBase64String(encryptedData)
        };

        // Post encrypted credentials to the server
        var response = await unauthorizedClient.PostAsJsonAsync("auth/signin", encryptedCredentials);

        return response;
    }

    public async Task InitializeAsync()
    {
        await _postgres.InitializeAsync();
        await _rabbitmq.InitializeAsync();
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();

        await _postgres.DisposeAsync();
        await _rabbitmq.DisposeAsync();
    }
}