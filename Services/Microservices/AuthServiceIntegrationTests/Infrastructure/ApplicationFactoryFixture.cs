using AuthNuget.Testing;
using AuthService;
using AuthService.Commands.NewUser;
using AuthServiceIntegrationTests.Infrastructure.TestContainer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using RabbitMqNuget.Registration;

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