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

    public static Result<Comment> Create(string content)
    {
        return new Comment
        {
            Content = content
        };
    }
}
