using MassTransit;
using Portfolio.Commands.Seedwork;
using Portfolio.Commands.Wallet;
using Portfolio.Consumers.Messages;

namespace Portfolio.Consumers;

public sealed class UserCreatedConsumer : IConsumer<UserCreated>
{
    private readonly ICommandDispatcher _commandDispatcher;

    public UserCreatedConsumer(ICommandDispatcher commandDispatcher)
    {
        _commandDispatcher = commandDispatcher;
    }

    public async Task Consume(ConsumeContext<UserCreated> context)
    {
        UserCreated userCreated = context.Message;

        var result = await _commandDispatcher.DispatchAsync(new CreateWallet(userCreated.WalletId));

        result.ThrowIfException();
    }
}