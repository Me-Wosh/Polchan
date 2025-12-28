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

    public static Result<Reaction> Create(ReactionType reactionType)
    {
        return new Reaction
        {
            ReactionType = reactionType
        };
    }
}
