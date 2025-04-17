using AuthNuget.Testing;
using FakeItEasy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Portfolio;
using Portfolio.Consumers.Messages;
using Portfolio.Proxies;
using Portfolio.Proxies.Dtos;
using Portfolio.Proxies.Impl;
using PortfolioServiceIntegrationTests.Infrastructure.TestContainer;
using RabbitMqNuget.Registration;

namespace PortfolioServiceIntegrationTests.Infrastructure;

public sealed class ApplicationFactoryFixture : SecurityApplicationFactoryFixture<Startup>, IAsyncLifetime
{
    private readonly Postgres _postgres = new();
    private readonly Rabbitmq _rabbitmq = new();

    private readonly StockProxy _stockProxy = A.Fake<StockProxy>();
    private readonly TimeProxy _timeProxy = A.Fake<TimeProxy>();
    private readonly AuthProxy _authProxy = A.Fake<AuthProxy>();

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
            A.CallTo(() => _stockProxy.GetStockPrice(A<string>._, A<DateTime>._))
                .Returns(new StockPrice(1_000));
            A.CallTo(() => _timeProxy.GetCurrentTime())
                .Returns(new CurrentTime(DateTime.UtcNow));
            A.CallTo(() => _authProxy.GetWalletIdAsync())
                .Returns(new WalletId("I'm a fake wallet Id"));

            services.AddScoped<IStockProxy, StockProxy>(_ => _stockProxy);
            services.AddScoped<ITimeProxy, TimeProxy>(_ => _timeProxy);
            services.AddScoped<IAuthProxy, AuthProxy>(_ => _authProxy);

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