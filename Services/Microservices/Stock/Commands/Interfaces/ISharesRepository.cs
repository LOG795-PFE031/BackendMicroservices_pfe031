namespace Stock.Commands.Interfaces;

public interface ISharesRepository
{
    Task<IEnumerable<Domain.Share>> GetAllAsync();
    Task<Domain.Share?> GetBySymbolAsync(string id);
    Task AddAsync(Domain.Share share);
    Task UpdateAsync(Domain.Share share);
    Task DeleteAsync(string id);
}