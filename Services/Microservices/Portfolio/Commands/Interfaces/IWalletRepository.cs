namespace Portfolio.Commands.Interfaces;

public interface IWalletRepository
{
    Task<Domain.Wallet?> GetAsync(string id);
    Task AddAsync(Domain.Wallet wallet);
    Task UpdateAsync(Domain.Wallet wallet);
}