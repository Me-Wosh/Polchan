using Ardalis.Result;
using Hangfire;
using MediatR;
using Polchan.Application.Interfaces;
using Polchan.Core.Interfaces;
using Polchan.Shared.MediatR;

namespace Polchan.Application.Threads;

public record DeleteThreadCommand(Guid Id) : ICommand<Unit>;

public class DeleteThreadHandler(
    IUserAccessor userAccessor,
    IPolchanDbContext dbContext,
    IBackgroundJobClient backgroundJobClient
) : ICommandHandler<DeleteThreadCommand, Unit>
{
    public async Task<Result<Unit>> Handle(DeleteThreadCommand command, CancellationToken cancellationToken)
    {
        var thread = await dbContext.Threads.FindAsync([command.Id], cancellationToken);

        if (thread is null)
            return Result.NotFound("Thread not found");

        var userId = userAccessor.GetUserId();

        if (!userId.IsSuccess)
            return userId.Map();

        var deleteResult = thread.Delete(userId);

        if (!deleteResult.IsSuccess)
            return deleteResult.Map();

        await dbContext.SaveChangesAsync(cancellationToken);

        backgroundJobClient.Enqueue<IThreadCleanupJob>(job => job.CleanupThreadAsync(thread.Id, CancellationToken.None));

        return Result.Success();
    }
}
