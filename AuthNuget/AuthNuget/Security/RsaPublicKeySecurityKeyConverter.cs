using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace AuthNuget.Security;

internal sealed class RsaPublicKeySecurityKeyConverter
{
    internal static RsaPublicKeySecurityKeyConverter? Instance { get; set; }

    internal RsaSecurityKey RsaSecurityKey { get; }

    internal RsaPublicKeySecurityKeyConverter(string publicKey)
    {
        var rsa = RSA.Create(2_048);

        rsa.ImportFromPem(publicKey.ToCharArray());

        RsaSecurityKey = new RsaSecurityKey(rsa);
    }
}