using System.Security.Cryptography;
using Ardalis.Result;
using Polchan.Application.Interfaces;

namespace Polchan.Infrastructure.Auth.Services;

public class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 128 / 8;
    private const int KeySize = 512 / 8;
    private const int Iterations = 100000;
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA512;

    public Result<string> Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, KeySize);

        return string.Join('-', Convert.ToHexString(hash), Convert.ToHexString(salt));
    }

    public Result<bool> VerifyHash(string passwordHash, string providedPassword)
    {
        var passwordParts = passwordHash.Split('-');
        var hash = Convert.FromHexString(passwordParts[0]);
        var salt = Convert.FromHexString(passwordParts[1]);

        var providedPasswordHash = Rfc2898DeriveBytes.Pbkdf2(providedPassword, salt, Iterations, Algorithm, KeySize);

        // Protection against timed attacks
        return CryptographicOperations.FixedTimeEquals(hash, providedPasswordHash);
    }
}
