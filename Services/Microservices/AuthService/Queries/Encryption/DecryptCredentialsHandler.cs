using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AuthNuget.Security;
using AuthService.Dtos;
using AuthService.Monads;
using AuthService.Queries.Seedwork;

namespace AuthService.Queries.Encryption;

public sealed class DecryptCredentialsHandler : IQueryHandler<DecryptCredentials, UserCredentials>
{
    public Task<Result<UserCredentials>> Handle(DecryptCredentials query, CancellationToken cancellation)
    {
        byte[] decryptedData;
        using (var rsa = RSA.Create())
        {
            rsa.ImportParameters(RsaKeyStorage.Instance.PrivateKey);
            decryptedData = rsa.Decrypt(Convert.FromBase64String(query.EncryptedData), RSAEncryptionPadding.OaepSHA256);
        }

        var jsonCredentials = Encoding.UTF8.GetString(decryptedData);

        var jsonSettings = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var userCredentials = JsonSerializer.Deserialize<UserCredentials>(jsonCredentials, jsonSettings)!;

        return Task.FromResult(Result.Success(userCredentials));
    }
}