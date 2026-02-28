using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
using Polchan.Core.Interfaces;
using Polchan.Core.Posts.Entities;
using Polchan.Core.Posts.Enums;

namespace Polchan.Core.Posts.Services;

public class PostReactionService(IPolchanDbContext dbContext)
{
    public async Task<Result> AddReactionAsync(
        Guid postId,
        Guid userId,
        ReactionType reactionType,
        CancellationToken cancellationToken
    )
    {
        var postExists = await dbContext.Posts.AnyAsync(c => c.Id == postId, cancellationToken);
            
        if (!postExists)
            return Result.NotFound("Post not found");

        var existingReaction = await dbContext
            .Reactions
            .FirstOrDefaultAsync(r => r.PostId == postId && r.OwnerId == userId, cancellationToken);

        if (existingReaction is null)
        {
            return Result.Success()
                .Bind(_ => Reaction.CreateForPost(reactionType, userId, postId))
                .Bind(reaction =>
                {
                    dbContext.Reactions.Add(reaction);
                    return Result.Success();
                });
        }

        if (existingReaction.ReactionType == reactionType)
            return Result.Invalid(new ValidationError("User has already reacted with this reaction type to this post"));

        return existingReaction.UpdateReactionType(reactionType);
    }

    public async Task<Result> RemoveReactionAsync(Guid reactionId, Guid ownerId, CancellationToken cancellationToken)
    {
        var reaction = await dbContext.Reactions.FindAsync([reactionId], cancellationToken);

        if (reaction is null)
            return Result.NotFound("Reaction not found");

        if (reaction.OwnerId != ownerId)
            return Result.Forbidden("User is not the owner of the reaction");

        dbContext.Reactions.Remove(reaction);
        return Result.Success();
    }
}
