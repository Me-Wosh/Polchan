using Ardalis.Result;
using Hangfire;
using MediatR;
using Polchan.Application.Interfaces;
using Polchan.Core.Interfaces;
using Polchan.Shared.MediatR;

namespace Polchan.Application.Posts;

public record DeleteCommentCommand(Guid Id) : ICommand<Unit>;

public class DeleteCommentHandler(
    IUserAccessor userAccessor,
    IPolchanDbContext dbContext,
    IBackgroundJobClient backgroundJobClient
) : ICommandHandler<DeleteCommentCommand, Unit>
{
    public async Task<Result<Unit>> Handle(DeleteCommentCommand command, CancellationToken cancellationToken)
    {
        var comment = await dbContext.Comments.FindAsync([command.Id], cancellationToken);

        if (comment is null)
            return Result.NotFound("Comment not found");

        var userId = userAccessor.GetUserId();

        if (!userId.IsSuccess)
            return userId.Map();

        var deleteResult = comment.Delete(userId);

        if (!deleteResult.IsSuccess)
            return deleteResult.Map();

        await dbContext.SaveChangesAsync(cancellationToken);
        
        backgroundJobClient.Enqueue<ICommentCleanupJob>(job => job.CleanupCommentAsync(comment.Id, CancellationToken.None));

        return Result.Success();
    }
}
