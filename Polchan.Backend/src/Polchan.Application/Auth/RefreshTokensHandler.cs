using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
using Polchan.Application.Auth.Services;
using Polchan.Infrastructure;
using Polchan.Shared.MediatR;

namespace Polchan.Application.Auth;

public record RefreshTokensCommand : ICommand<string>;

public class RefreshTokensHandler(
    IUserAccessor userAccessor,
    PolchanDbContext dbContext,
    ITokensService tokensService
) : ICommandHandler<RefreshTokensCommand, string>
{
    public async Task<Result<string>> Handle(RefreshTokensCommand command, CancellationToken cancellationToken)
    {
        var refreshToken = userAccessor.GetRefreshToken();
    
        if (!refreshToken.IsSuccess)
            return refreshToken.Map();

        var user = await dbContext
            .Users
            .Include(u => u.RefreshTokens)
            .Where(u => u.RefreshTokens.Any(rt => rt.Token == refreshToken))
            .SingleOrDefaultAsync(cancellationToken);

        if (user is null)
            return Result.Unauthorized("Invalid refresh token. Check the refresh token or login again");

        await dbContext
            .RefreshTokens
            .Where(rt => rt.UserId == user.Id)
            .Where(rt => rt.ExpirationDate <= DateTime.UtcNow)
            .ExecuteDeleteAsync(cancellationToken);

        var unexpiredRefreshToken = user.RefreshTokens.SingleOrDefault(t => t.Token == refreshToken);

        if (unexpiredRefreshToken is null)
            return Result.Unauthorized("Invalid refresh token. Check the refresh token or login again");

        var jwt = tokensService.CreateJwt(user);

        return await Result.Success()
            .Bind(_ => jwt)
            .Bind(_ => tokensService.CreateRefreshToken())
            .Bind(token => user.ReplaceRefreshToken(unexpiredRefreshToken.Id, token))
            .Bind(refreshToken => userAccessor.SetRefreshToken(refreshToken))
            .BindAsync(async _ =>
            {
                await dbContext.SaveChangesAsync(cancellationToken);
                return Result.Success(jwt);
            });
    }
}
