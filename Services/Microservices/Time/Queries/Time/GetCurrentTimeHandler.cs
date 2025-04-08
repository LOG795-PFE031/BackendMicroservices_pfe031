using Time.Commands.Interfaces;
using Time.Domain;
using Time.Domain.Monads;
using Time.Queries.Seedwork;

namespace Time.Queries.Time;

public sealed class GetCurrentTimeHandler : IQueryHandler<GetCurrentTime, DateTime>
{
    private readonly IInMemoryStore<Clock> _memoryStore;

    public GetCurrentTimeHandler(IInMemoryStore<Clock> memoryStore)
    {
        _memoryStore = memoryStore;
    }

    public Task<Result<DateTime>> Handle(GetCurrentTime query, CancellationToken cancellation)
    {
        var clock = _memoryStore.Values.Single();

        return Task.FromResult(Result.Success(clock.DateTime));
    }
}