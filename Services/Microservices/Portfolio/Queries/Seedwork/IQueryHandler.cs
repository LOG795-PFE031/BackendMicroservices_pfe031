using Portfolio.Domain.Monads;

namespace Portfolio.Queries.Seedwork;

public interface IQueryHandler<in TQuery, TQueryResult> where TQuery : IQuery
{
    Task<Result<TQueryResult>> Handle(TQuery query, CancellationToken cancellation);
}