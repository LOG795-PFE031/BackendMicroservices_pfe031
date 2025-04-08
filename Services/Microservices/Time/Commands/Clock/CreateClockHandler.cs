using Time.Commands.Interfaces;
using Time.Commands.Seedwork;
using Time.Domain.Monads;

namespace Time.Commands.Clock;

public sealed class CreateClockHandler : ICommandHandler<CreateClock>
{
    private readonly IInMemoryStore<Domain.Clock> _memoryStore;

    private readonly object _lock = new();

    public CreateClockHandler(IInMemoryStore<Domain.Clock> memoryStore)
    {
        _memoryStore = memoryStore;
    }

    public Task<Result> Handle(CreateClock command, CancellationToken cancellation)
    {
        lock (_lock)
        {
            if (_memoryStore.Values.IsEmpty)
            {
                var newClock = new Domain.Clock();

                _memoryStore.AddOrUpdate(newClock.Id, newClock);
            }
            else
            {
                return Task.FromResult(Result.Failure("Clock already exists"));
            }
        }

        return Task.FromResult(Result.Success());
    }
}