using Portfolio.Commands.Seedwork;
using Portfolio.Domain.ValueObjects;

namespace Portfolio.Commands.ShareVolume;

public record ModifyShareVolume(string Symbol, Money Price, int Volume, string WalletId) : ICommand;