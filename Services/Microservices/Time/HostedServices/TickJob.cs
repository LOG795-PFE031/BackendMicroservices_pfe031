using RabbitMqNuget.Services;
using Time.Commands.Clock;
using Time.Commands.Seedwork;
using Time.Commands.Time;

namespace Time.HostedServices;

public sealed class TickJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    private const int TickIntervalMs = 1_000;

    public TickJob(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();

        var commandDispatcher = scope.ServiceProvider.GetRequiredService<ICommandDispatcher>();
        var transactionInfo = scope.ServiceProvider.GetRequiredService<ITransactionInfo>();

        var result = await commandDispatcher.DispatchAsync(new CreateClock(), stoppingToken);

        result.ThrowIfException();

        while (stoppingToken.IsCancellationRequested is false)
        {
            transactionInfo.CorrelationId = Guid.NewGuid();

            await commandDispatcher.DispatchAsync(new ClockTick(), stoppingToken);

            await Task.Delay(TickIntervalMs, stoppingToken);
        }
    }
}