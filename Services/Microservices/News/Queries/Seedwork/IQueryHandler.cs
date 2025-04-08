using News.Domain.Monads;

namespace News.Queries.Seedwork;

public interface IQueryHandler<in TQuery, TQueryResult> where TQuery : IQuery
{
    Task<Result<TQueryResult>> Handle(TQuery query, CancellationToken cancellation);
}