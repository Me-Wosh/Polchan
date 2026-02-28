using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Polchan.Application.Interfaces;
using Polchan.Core.Interfaces;
using Polchan.Core.Posts.Entities;
using Polchan.Shared.MediatR;

namespace Polchan.Application.Posts;

public record CreateCommentCommand(Guid PostId, string Content) : ICommand<Unit>;

public class CreateCommentHandler(
    IUserAccessor userAccessor,
    IPolchanDbContext dbContext
) : ICommandHandler<CreateCommentCommand, Unit>
{
    public async Task<Result<Unit>> Handle(CreateCommentCommand command, CancellationToken cancellationToken)
    {
        var userId = userAccessor.GetUserId();

        if (!userId.IsSuccess)
            return userId.Map();

        var user = await dbContext.Users.FindAsync([userId], cancellationToken);

        if (user is null)
            return Result.Unauthorized();

        var postExists = await dbContext.Posts.AnyAsync(p => p.Id == command.PostId, cancellationToken);

        if (!postExists)
            return Result.NotFound("Post not found");

        return await Result.Success()
            .Bind(_ => Comment.Create(command.Content, command.PostId, userId))
            .BindAsync(async comment =>
            {
                dbContext.Comments.Add(comment);
                await dbContext.SaveChangesAsync(cancellationToken);
                return Result.Success();
            });
    }
}
