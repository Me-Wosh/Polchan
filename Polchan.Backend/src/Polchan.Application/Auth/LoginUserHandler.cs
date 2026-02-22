using System.ComponentModel.DataAnnotations;
using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
using Polchan.Application.Interfaces;
using Polchan.Shared.MediatR;

namespace Polchan.Application.Auth;

public record LoginUserCommand([Required] string Email, [Required] string Password) : ICommand<string>;

public class LoginUserHandler(
    IPasswordHasher passwordHasher,
    IPolchanDbContext dbContext,
    ITokensService tokensService,
    IUserAccessor userAccessor
) : ICommandHandler<LoginUserCommand, string>
{
    public async Task<Result<string>> Handle(LoginUserCommand command, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users.SingleOrDefaultAsync(u => u.Email == command.Email, cancellationToken);

        if (user is null)
            return Result.Unauthorized("Invalid e-mail or password");

        var hashResult = passwordHasher.VerifyHash(user.PasswordHash, command.Password);

        if (!hashResult.IsSuccess)
            return hashResult.Map();
        
        if (!hashResult)
            return Result.Unauthorized("Invalid e-mail or password");

        var token = tokensService.CreateToken(user);

        return await Result.Success()
            .Bind(_ => token)
            .Bind(_ => tokensService.CreateRefreshToken())
            .Bind(token => user.AddRefreshToken(token))
            .Bind(refreshToken => userAccessor.SetRefreshToken(refreshToken))
            .BindAsync(async _ =>
            {
                await dbContext.SaveChangesAsync(cancellationToken);
                return Result.Success(token);
            });
    }
}