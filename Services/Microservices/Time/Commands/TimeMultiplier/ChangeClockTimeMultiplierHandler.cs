using Time.Commands.Interfaces;
using Time.Commands.Seedwork;
using Time.Domain.Monads;

namespace Time.Commands.TimeMultiplier;

public sealed class ChangeClockTimeMultiplierHandler : ICommandHandler<ChangeClockTimeMultiplier>
{
    private readonly IInMemoryStore<Domain.Clock> _memoryStore;

    public ChangeClockTimeMultiplierHandler(IInMemoryStore<Domain.Clock> memoryStore)
    {
        _memoryStore = memoryStore;
    }

    public Task<Result> Handle(ChangeClockTimeMultiplier command, CancellationToken cancellation)
    {
        var clock = _memoryStore.Values.Single();

        clock.SetMultiplier(command.Multiplier);

        return Task.FromResult(Result.Success());
    }
}