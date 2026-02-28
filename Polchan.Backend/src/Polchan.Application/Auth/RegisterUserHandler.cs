using System.ComponentModel.DataAnnotations;
using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Polchan.Application.Interfaces;
using Polchan.Core.Interfaces;
using Polchan.Core.Users.Entities;
using Polchan.Core.Users.Enums;
using Polchan.Shared.MediatR;

namespace Polchan.Application.Auth;

public record RegisterUserCommand(
    [Required] string Email,
    [Required] string Username,
    [Required] string Password
) : ICommand<Unit>;

public class RegisterUserHandler(
    IPasswordHasher passwordHasher,
    IPolchanDbContext dbContext
) : ICommandHandler<RegisterUserCommand, Unit>
{
    public async Task<Result<Unit>> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        var email = await dbContext
            .Users
            .AsNoTracking()
            .Select(u => u.Email)
            .SingleOrDefaultAsync(e => e == command.Email, cancellationToken);

        if (email is not null)
            return Result.Error("User with this e-mail already exists");

        return await Result.Success()
            .Bind(_ => passwordHasher.Hash(command.Password.Trim()))
            .Bind(password => User.Create(command.Email, command.Username, password, UserRole.User))
            .BindAsync(async user =>
            {
                await dbContext.Users.AddAsync(user, cancellationToken);
                await dbContext.SaveChangesAsync(cancellationToken);
                return Result.Success();
            });
    }
}
