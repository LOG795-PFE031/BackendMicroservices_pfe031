using Portfolio.Commands.Seedwork;

namespace Portfolio.Commands.Wallet;

public record CreateWallet(string WalletId) : ICommand;