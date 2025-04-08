using Stock.Domain.Monads;

namespace Stock.Commands.Seedwork;

public interface ICommandDispatcher
{
    Task<Result> DispatchAsync<TCommand>(TCommand command, CancellationToken cancellation = default) where TCommand : ICommand;
}