using Ardalis.Result;
using MediatR;
using Polchan.Application.Interfaces;
using Polchan.Core.Interfaces;
using Polchan.Core.Posts.Enums;
using Polchan.Core.Posts.Services;
using Polchan.Shared.MediatR;

namespace Polchan.Application.Posts;

public record RemoveReactionCommand(ReactionTarget Target, Guid Id) : ICommand<Unit>;

public class RemoveReactionHandler(
    IUserAccessor userAccessor,
    PostReactionService postReactionService,
    CommentReactionService commentReactionService,
    IPolchanDbContext dbContext
) : ICommandHandler<RemoveReactionCommand, Unit>
{
    public async Task<Result<Unit>> Handle(RemoveReactionCommand command, CancellationToken cancellationToken)
    {
        var userId = userAccessor.GetUserId();

        if (!userId.IsSuccess)
            return userId.Map();
        
        return await Result.Success()
            .BindAsync(async _ =>
                command.Target == ReactionTarget.Post
                    ? await postReactionService.RemoveReactionAsync(command.Id, userId, cancellationToken)
                    : await commentReactionService.RemoveReactionAsync(command.Id, userId, cancellationToken)
            )
            .BindAsync(async _ =>
            {
                await dbContext.SaveChangesAsync(cancellationToken);
                return Result.Success();
            });
    }
}
