﻿using Portfolio.Commands.Interfaces;
using Portfolio.Commands.Seedwork;
using Portfolio.Domain.Monads;
using Portfolio.Domain.ValueObjects;

namespace Portfolio.Commands.Wallet;

public sealed class CreateWalletHandler : ICommandHandler<CreateWallet>   
{
    private readonly IWalletRepository _walletRepository;

    public CreateWalletHandler(IWalletRepository walletRepository)
    {
        _walletRepository = walletRepository;
    }

    public async Task<Result> Handle(CreateWallet command, CancellationToken cancellation)
    {
        await _walletRepository.AddAsync(new Domain.Wallet(command.WalletId, new Money(100_000), []));

        return Result.Success();
    }
}