using AuthNuget.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using News;
using News.Commands.Seedwork;
using News.Consumers;
using NewsServiceIntegrationTests.Infrastructure.TestContainer;
using RabbitMqNuget.Registration;
using RabbitMqNuget.Testing;

namespace NewsServiceIntegrationTests.Infrastructure;

public sealed class ApplicationFactoryFixture : SecurityApplicationFactoryFixture<Startup>, IAsyncLifetime
{
    private readonly Azurite _azurite = new();
    private readonly Mongodb _mongodb = new();
    private readonly Rabbitmq _rabbitmq = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureAppConfiguration((_, config) =>
        {
            var integrationConfig = new Dictionary<string, string>
            {
                ["ConnectionStrings:Blob"] = _azurite.Container.GetConnectionString(),
                ["ConnectionStrings:Mongodb"] = _mongodb.Container.GetConnectionString(),
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
                    .AddPublisher<News.Consumers.Messages.News>("news-exchange")
                    .AddConsumer<News.Consumers.Messages.News, ConsumerDecorator<News.Consumers.Messages.News, NewsConsumer>>("news-exchange", sp =>
                    {
                        var scope = sp.CreateScope();
                        return new(new(scope.ServiceProvider.GetRequiredService<ICommandDispatcher>()));
                    }));
        });
    }
    public async Task InitializeAsync()
    {
        await _azurite.InitializeAsync();
        await _mongodb.InitializeAsync();
        await _rabbitmq.InitializeAsync();
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();

        await _azurite.DisposeAsync();
        await _mongodb.DisposeAsync();
        await _rabbitmq.DisposeAsync();
    }
}