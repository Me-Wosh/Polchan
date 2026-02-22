using System.ComponentModel.DataAnnotations;
using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Polchan.Application.Interfaces;
using Polchan.Core.Threads.Enums;
using Polchan.Shared.MediatR;
using Thread = Polchan.Core.Threads.Entities.Thread;

namespace Polchan.Application.Threads;

public record CreateThreadCommand([Required] string Name, [Required] ThreadCategory Category) : ICommand<Unit>;

public class CreateThreadHandler(
    IUserAccessor userAccessor,
    IPolchanDbContext dbContext
) : ICommandHandler<CreateThreadCommand, Unit>
{
    public async Task<Result<Unit>> Handle(CreateThreadCommand command, CancellationToken cancellationToken)
    {
        var userId = userAccessor.GetUserId();

        if (!userId.IsSuccess)
            return userId.Map();

        var user = await dbContext.Users.SingleOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
            return Result.Unauthorized();

        var threadName = command.Name.Trim();

        var threadExists = await dbContext.Threads.AnyAsync(t => t.Name == threadName, cancellationToken);

        if (threadExists)
            return Result.Error("Thread with this name already exists");

        return await Result.Success()
            .Bind(_ => Thread.Create(threadName, user, command.Category))
            .Bind(thread => user.AddOwnedThread(thread))
            .BindAsync(async _ =>
            {
                await dbContext.SaveChangesAsync(cancellationToken);
                return Result.Success();
            });
    }
}
