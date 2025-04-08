using Portfolio.Queries.Seedwork;

namespace Portfolio.Queries.ShareVolume;

public record GetSharesVolume(string WalletId) : IQuery;