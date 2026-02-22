using Ardalis.Result;
using Polchan.Core.Users.Entities;

namespace Polchan.Application.Interfaces;

public interface ITokensService
{
    Result<string> CreateToken(User user);
    Result<string> CreateRefreshToken();
}
