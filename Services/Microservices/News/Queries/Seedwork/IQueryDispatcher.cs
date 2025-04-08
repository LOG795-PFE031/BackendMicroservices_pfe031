using News.Domain.Monads;

namespace News.Queries.Seedwork;

public interface IQueryDispatcher
{
    Task<Result<TQueryResult>> DispatchAsync<TQuery, TQueryResult>(TQuery query, CancellationToken cancellation = default)
        where TQuery : IQuery;
}