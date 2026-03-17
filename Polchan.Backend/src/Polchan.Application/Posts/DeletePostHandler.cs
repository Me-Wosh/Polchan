using Ardalis.Result;
using Hangfire;
using MediatR;
using Polchan.Application.Interfaces;
using Polchan.Core.Interfaces;
using Polchan.Shared.MediatR;

namespace Polchan.Application.Posts;

public record DeletePostCommand(Guid PostId) : ICommand<Unit>;

public class DeletePostHandler(
    IUserAccessor userAccessor,
    IPolchanDbContext dbContext,
    IBackgroundJobClient backgroundJobClient
) : ICommandHandler<DeletePostCommand, Unit>
{
    public async Task<Result<Unit>> Handle(DeletePostCommand command, CancellationToken cancellationToken)
    {
        var post = await dbContext.Posts.FindAsync([command.PostId], cancellationToken);

        if (post is null)
            return Result.NotFound("Post not found");

        var userId = userAccessor.GetUserId();

        if (!userId.IsSuccess)
            return userId.Map();

        var deleteResult = post.Delete(userId);

        if (!deleteResult.IsSuccess)
            return deleteResult.Map();

        await dbContext.SaveChangesAsync(cancellationToken);

        backgroundJobClient.Enqueue<IPostCleanupJob>(job => job.CleanupPostAsync(post.Id, CancellationToken.None));

        return Result.Success();
    }
}
