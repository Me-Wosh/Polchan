using System.ComponentModel.DataAnnotations;
using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Polchan.Application.Auth.Services;
using Polchan.Infrastructure;
using Polchan.Shared.MediatR;

namespace Polchan.Application.Threads;

public record UnsubscribeThreadCommand([Required] Guid Id) : ICommand<Unit>;

public class UnsubscribeThreadHandler(
    IUserAccessor userAccessor,
    PolchanDbContext dbContext
) : ICommandHandler<UnsubscribeThreadCommand, Unit>
{
    public async Task<Result<Unit>> Handle(UnsubscribeThreadCommand command, CancellationToken cancellationToken)
    {
        var userId = userAccessor.GetUserId();

        if (!userId.IsSuccess)
            return userId.Map();

        var user = await dbContext.Users
            .Include(u => u.SubscribedThreads)
            .SingleOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
            return Result.Unauthorized();
            
        var thread = await dbContext.Threads.SingleOrDefaultAsync(t => t.Id == command.Id, cancellationToken);

        if (thread is null)
            return Result.NotFound($"Thread with id: '{command.Id}' not found");

        return await Result.Success()
            .Bind(_ => user.UnsubscribeThread(thread.Id))
            .BindAsync(async _ =>
            {
                await dbContext.SaveChangesAsync(cancellationToken);
                return Result.Success();
            });
    }
}
