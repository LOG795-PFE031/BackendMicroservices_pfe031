using Portfolio.Domain.Monads;

namespace Portfolio.Commands.Seedwork;

public interface ICommandDispatcher
{
    Task<Result> DispatchAsync<TCommand>(TCommand command, CancellationToken cancellation = default) where TCommand : ICommand;
}