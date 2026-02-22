using Ardalis.Result;

namespace Polchan.Application.Interfaces;

public interface IPasswordHasher
{
    Result<string> Hash(string password);
    Result<bool> VerifyHash(string passwordHash, string providedPassword);
}
