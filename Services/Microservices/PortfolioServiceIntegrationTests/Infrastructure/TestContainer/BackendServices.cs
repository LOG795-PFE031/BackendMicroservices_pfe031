using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace PortfolioServiceIntegrationTests.Infrastructure.TestContainer;

public sealed class BackendServices : IAsyncLifetime
{
    private readonly IContainer _authService = new ContainerBuilder().WithImage("auth-service:latest")
        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(8081))
        .WithExposedPort(8081)
        .Build();

    private readonly IContainer _timeService = new ContainerBuilder().WithImage("time-service:latest")
        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(8081))
        .WithExposedPort(8081)
        .Build();

    private readonly IContainer _stockService = new ContainerBuilder().WithImage("stock-service:latest")
        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(8081))
        .WithExposedPort(8081)
        .Build();

    public Task InitializeAsync()
    {
        return Task.WhenAll(_authService.StartAsync(), _timeService.StartAsync(), _stockService.StartAsync());
    }
    public async Task DisposeAsync()
    {
        await Task.WhenAll(_authService.DisposeAsync().AsTask(), _timeService.DisposeAsync().AsTask(), _stockService.DisposeAsync().AsTask());
    }
}