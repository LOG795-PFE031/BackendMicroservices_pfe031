using AuthService.Queries.Seedwork;

namespace AuthService.Queries.Encryption;

public sealed record DecryptCredentials(string EncryptedData) : IQuery;