using Portfolio.Domain.Monads;

namespace Portfolio.Commands.Seedwork;

public interface ICommandHandler<in TCommand> where TCommand : ICommand
{
    Task<Result> Handle(TCommand command, CancellationToken cancellation);
}