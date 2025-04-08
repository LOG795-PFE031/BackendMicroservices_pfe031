using RabbitMqNuget.Services;
using Time.Commands.Interfaces;
using Time.Commands.Seedwork;
using Time.Domain.DomainEvents;
using Time.Domain.Monads;

namespace Time.Commands.Time;

public sealed class ClockTickHandler : ICommandHandler<ClockTick>
{
    private readonly IInMemoryStore<Domain.Clock> _memoryStore;
    private readonly IMessagePublisher<DayStarted> _messagePublisher;

    public ClockTickHandler(IInMemoryStore<Domain.Clock> memoryStore, IMessagePublisher<DayStarted> messagePublisher)
    {
        _memoryStore = memoryStore;
        _messagePublisher = messagePublisher;
    }

    public async Task<Result> Handle(ClockTick command, CancellationToken cancellation)
    {
        var clock = _memoryStore.Values.Single();

        clock.Tick();

        foreach (var domainEvent in clock.DomainEvents.Where(@event => @event is DayStarted).Cast<DayStarted>())
        {
            await _messagePublisher.Publish(domainEvent);
        }

        return Result.Success();
    }
}