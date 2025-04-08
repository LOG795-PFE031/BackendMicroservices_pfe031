using AuthNuget.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using RabbitMqNuget.Registration;
using Time;
using Time.Domain.DomainEvents;
using TimeServiceIntegrationTests.Infrastructure.TestContainer;
using TimeServiceIntegrationTests.Tests.Time.Consumers;

namespace TimeServiceIntegrationTests.Infrastructure;

public sealed class ApplicationFactoryFixture : SecurityApplicationFactoryFixture<Startup>, IAsyncLifetime
{
    private readonly Rabbitmq _rabbitmq = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureAppConfiguration((_, config) =>
        {
            var integrationConfig = new Dictionary<string, string>
            {
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
                    .AddPublisher<DayStarted>("day-started-exchange")
                    .AddConsumer<DayStarted, TestTimeMessageConsumer>("day-started-exchange"));
        });
    }

    public async Task InitializeAsync()
    {
        await _rabbitmq.InitializeAsync();
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();

        await _rabbitmq.DisposeAsync();
    }
}