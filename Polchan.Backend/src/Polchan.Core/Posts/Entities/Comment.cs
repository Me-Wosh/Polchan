using Ardalis.Result;
using Polchan.Core.Users.Entities;

namespace Polchan.Core.Posts.Entities;

public class Comment : BaseEntity
{
    private readonly List<Reaction> _reactions = [];

    private Comment() { }

    public string Content { get; private set; } = string.Empty;
    
    public Guid PostId { get; private set; }
    public Post Post { get; private set; } = null!;

    public Guid OwnerId { get; private set; }
    public User Owner { get; private set; } = null!;

    public IReadOnlyCollection<Reaction> Reactions => _reactions; 

    public static Result<Comment> Create(string content, Guid postId, Guid ownerId)
    {
        var comment = new Comment();

        return Result.Success()
            .Bind(_ => comment.UpdateContent(content))
            .Bind(_ => comment.UpdatePostId(postId))
            .Bind(_ => comment.UpdateOwnerId(ownerId));
    }

    public Result<Comment> UpdateContent(string content)
    {
        content = content.Trim();

        var errors = new List<ValidationError>();

        if (string.IsNullOrEmpty(content))
            errors.Add(new ValidationError("Content cannot be empty"));

        if (content.Length > 255)
            errors.Add(new ValidationError("Content cannot exceed 255 characters"));

        if (errors.Count > 0)
            return Result.Invalid(errors);

        Content = content;
        return Result.Success(this);
    }

    private Result<Comment> UpdatePostId(Guid postId)
    {
        if (postId == Guid.Empty)
            return Result.Invalid(new ValidationError("PostId cannot be empty"));

        PostId = postId;
        return Result.Success(this);
    }

    private Result<Comment> UpdateOwnerId(Guid ownerId)
    {
        if (ownerId == Guid.Empty)
            return Result.Invalid(new ValidationError("OwnerId cannot be empty"));

        OwnerId = ownerId;
        return Result.Success(this);
    }
}
