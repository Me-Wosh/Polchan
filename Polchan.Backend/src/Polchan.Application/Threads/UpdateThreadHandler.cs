using System.ComponentModel.DataAnnotations;
using Ardalis.Result;
using MediatR;
using Polchan.Application.Interfaces;
using Polchan.Core.Interfaces;
using Polchan.Core.Threads.Enums;
using Polchan.Shared.MediatR;

namespace Polchan.Application.Threads;

public record UpdateThreadCommand([Required] Guid Id, string? Name, ThreadCategory? Category) : ICommand<Unit>;

public class UpdateThreadHandler(
    IUserAccessor userAccessor,
    IPolchanDbContext dbContext
) : ICommandHandler<UpdateThreadCommand, Unit>
{
    public async Task<Result<Unit>> Handle(UpdateThreadCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(command.Name) && command.Category is null)
            return Result.Error("At least one parameter to update is required");

        var userId = userAccessor.GetUserId();

        if (!userId.IsSuccess)
            return userId.Map();

        var thread = await dbContext.Threads.FindAsync([command.Id], cancellationToken);

        if (thread is null)
            return Result.NotFound("Thread not found");

        if (thread.OwnerId != userId)
            return Result.Forbidden("User is not the owner of the thread");

        if (!string.IsNullOrEmpty(command.Name))
        {
            var updateNameResult = thread.UpdateName(command.Name);

            if (!updateNameResult.IsSuccess)
                return updateNameResult.Map();
        }

        if (command.Category is not null)
        {
            var updateCategoryResult = thread.UpdateCategory(command.Category.Value);

            if (!updateCategoryResult.IsSuccess)
                return updateCategoryResult.Map();
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
