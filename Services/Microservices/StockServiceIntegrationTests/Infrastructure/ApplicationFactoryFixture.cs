using AuthNuget.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMqNuget.Registration;
using RabbitMqNuget.Testing;
using Stock;
using Stock.Commands.Seedwork;
using Stock.Consumers.Messages;
using Stock.Consumers;
using StockServiceIntegrationTests.Infrastructure.TestContainer;

namespace StockServiceIntegrationTests.Infrastructure;

public sealed class ApplicationFactoryFixture : SecurityApplicationFactoryFixture<Startup>, IAsyncLifetime
{
    private readonly Mongodb _mongoDb = new();
    private readonly Rabbitmq _rabbitmq = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureAppConfiguration((_, config) =>
        {
            var integrationConfig = new Dictionary<string, string>
            {
                ["ConnectionStrings:Mongodb"] = _mongoDb.Container.GetConnectionString(),
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
                    .AddPublisher<StockQuote>("quote-exchange")
                    .AddConsumer<StockQuote, ConsumerDecorator<StockQuote, QuoteConsumer>>("quote-exchange", sp =>
                    {
                        var scope = sp.CreateScope();
                        return new(new QuoteConsumer(scope.ServiceProvider.GetRequiredService<ICommandDispatcher>()));
                    }));
        });
    }

    public async Task InitializeAsync()
    {
        await _mongoDb.InitializeAsync();
        await _rabbitmq.InitializeAsync();
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();

        await _mongoDb.DisposeAsync();
        await _rabbitmq.DisposeAsync();
    }
}