using System.Security.Cryptography;
using System.Text;

namespace Innovia.Api.Common.Auth.RefreshTokens;

public static class RefreshTokenHasher
{
    public static string Hash(string rawToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(bytes);
    }
}