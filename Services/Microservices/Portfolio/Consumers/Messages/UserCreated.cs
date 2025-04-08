using Portfolio.Domain.Seedwork.Abstract;

namespace Portfolio.Consumers.Messages;

public sealed class UserCreated : Event
{
    public string WalletId { get; init; } = string.Empty;
}