using Portfolio.Commands.Interfaces;
using Portfolio.Commands.Seedwork;
using Portfolio.Domain.Monads;

namespace Portfolio.Commands.ShareVolume;

public sealed class ModifyShareVolumeHandler : ICommandHandler<ModifyShareVolume>
{
    private readonly IWalletRepository _walletRepository;

    public ModifyShareVolumeHandler(IWalletRepository walletRepository)
    {
        _walletRepository = walletRepository;
    }

    public async Task<Result> Handle(ModifyShareVolume command, CancellationToken cancellation)
    {
         Domain.Wallet? wallet = await _walletRepository.GetAsync(command.WalletId);

        if (wallet is null) return Result.Failure("Wallet not found");

        Result result = int.IsPositive(command.Volume) ? 
            wallet.TryBuyStock(command.Symbol, command.Price, command.Volume) : 
            wallet.TrySellStock(command.Symbol, command.Price, Math.Abs(command.Volume));

        if (result.IsFailure()) return result;

        await _walletRepository.UpdateAsync(wallet);

        return Result.Success();
    }
}