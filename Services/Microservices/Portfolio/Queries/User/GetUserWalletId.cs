using Portfolio.Queries.Seedwork;

namespace Portfolio.Queries.User;

public record GetUserWalletId(string Username) : IQuery;