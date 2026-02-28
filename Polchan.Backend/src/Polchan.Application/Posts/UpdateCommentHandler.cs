using System.ComponentModel.DataAnnotations;
using Ardalis.Result;
using MediatR;
using Polchan.Application.Interfaces;
using Polchan.Core.Interfaces;
using Polchan.Shared.MediatR;

namespace Polchan.Application.Posts;

public record UpdateCommentCommand([Required] Guid Id, [Required] string Content) : ICommand<Unit>;

public class UpdateCommentHandler(
    IUserAccessor userAccessor,
    IPolchanDbContext dbContext
) : ICommandHandler<UpdateCommentCommand, Unit>
{
    public async Task<Result<Unit>> Handle(UpdateCommentCommand command, CancellationToken cancellationToken)
    {
        var userId = userAccessor.GetUserId();

        if (!userId.IsSuccess)
            return userId.Map();

        var comment = await dbContext.Comments.FindAsync([command.Id], cancellationToken);

        if (comment is null)
            return Result.NotFound("Comment not found");

        if (comment.OwnerId != userId)
            return Result.Forbidden("User is not the owner of the comment");

        return await Result.Success()
            .Bind(_ => comment.UpdateContent(command.Content))
            .BindAsync(async _ =>
            {
                await dbContext.SaveChangesAsync(cancellationToken);
                return Result.Success();
            });
    }
}
