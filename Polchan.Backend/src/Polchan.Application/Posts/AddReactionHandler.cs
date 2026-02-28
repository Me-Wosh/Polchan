using Ardalis.Result;
using MediatR;
using Polchan.Application.Interfaces;
using Polchan.Core.Interfaces;
using Polchan.Core.Posts.Enums;
using Polchan.Core.Posts.Services;
using Polchan.Shared.MediatR;

namespace Polchan.Application.Posts;

public record AddReactionCommand(ReactionTarget Target, Guid TargetId, ReactionType ReactionType) : ICommand<Unit>;

public class AddReactionHandler(
    IUserAccessor userAccessor,
    IPolchanDbContext dbContext,
    PostReactionService postReactionService,
    CommentReactionService commentReactionService
) : ICommandHandler<AddReactionCommand, Unit>
{
    public async Task<Result<Unit>> Handle(AddReactionCommand command, CancellationToken cancellationToken)
    {
        var userId = userAccessor.GetUserId();

        if (!userId.IsSuccess)
            return userId.Map();

        var user = await dbContext.Users.FindAsync([userId], cancellationToken);

        if (user is null)
            return Result.Unauthorized();

        return await Result.Success()
            .BindAsync(async _ =>
            {
                return command.Target == ReactionTarget.Post
                    ? await postReactionService.AddReactionAsync(command.TargetId, userId, command.ReactionType, cancellationToken)
                    : await commentReactionService.AddReactionAsync(command.TargetId, userId, command.ReactionType, cancellationToken);
            })         
            .BindAsync(async _ =>
            {
                await dbContext.SaveChangesAsync(cancellationToken);
                return Result.Success(Unit.Value);
            });
    }
}
