using Ardalis.Result;
using Polchan.Core.Users.Entities;

namespace Polchan.Application.Interfaces;

public interface IUserAccessor
{
    public Result<Guid> GetUserId();
    public Result<string> GetRefreshToken();
    public Result SetRefreshToken(RefreshToken refreshToken);
}
