using Ardalis.Result;
using Polchan.Core.Posts.Enums;
using Polchan.Core.Users.Entities;

namespace Polchan.Core.Posts.Entities;

public class Reaction : BaseEntity
{
    private Reaction() { }

    public ReactionType ReactionType { get; private set; }

    public Guid OwnerId { get; private set; }
    public User Owner { get; private set; } = null!;
    
    public Guid? PostId { get; private set; }
    public Post? Post { get; private set; }

    public Guid? CommentId { get; private set; }
    public Comment? Comment { get; private set; }

    public static Result<Reaction> CreateForPost(ReactionType reactionType, Guid ownerId, Guid postId)
    {
        return new Reaction
        {
            ReactionType = reactionType,
            OwnerId = ownerId,
            PostId = postId
        };
    }

    public static Result<Reaction> CreateForComment(ReactionType reactionType, Guid ownerId, Guid commentId)
    {
        return new Reaction
        {
            ReactionType = reactionType,
            OwnerId = ownerId,
            CommentId = commentId
        };
    }

    public Result UpdateReactionType(ReactionType reactionType)
    {
        ReactionType = reactionType;
        return Result.Success();
    }
}
