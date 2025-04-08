using Portfolio.Domain.Monads;

namespace Portfolio.Queries.Seedwork;

public interface IQueryDispatcher
{
    Task<Result<TQueryResult>> DispatchAsync<TQuery, TQueryResult>(TQuery query, CancellationToken cancellation = default)
        where TQuery : IQuery;
}