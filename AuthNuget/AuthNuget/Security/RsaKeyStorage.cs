using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace AuthNuget.Security;

public sealed class RsaKeyStorage
{
    public static RsaKeyStorage Instance { get; } = new();

    public RsaSecurityKey RsaSecurityKey { get; }

    public RSAParameters PrivateKey { get; }

    public string PublicKey { get; }

    private RsaKeyStorage()
    {
        var rsa = RSA.Create(2_048);
        
        PrivateKey = rsa.ExportParameters(true);
        PublicKey = rsa.ExportRSAPublicKeyPem();

        RsaSecurityKey = new RsaSecurityKey(rsa);
    }
}