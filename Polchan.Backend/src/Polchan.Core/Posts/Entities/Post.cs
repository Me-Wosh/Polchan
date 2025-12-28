using Ardalis.Result;
using Polchan.Core.Resources.Entities;
using Polchan.Core.Users.Entities;
using Thread = Polchan.Core.Threads.Entities.Thread;

namespace Polchan.Core.Posts.Entities;

public class Post : BaseEntity
{
    private readonly List<Resource> _images = [];
    private readonly List<Reaction> _reactions = [];
    private readonly List<Comment> _comments = [];
    
    private Post() { }

    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    public Guid ThreadId { get; private set; }
    public Thread Thread { get; private set; } = null!;

    public Guid OwnerId { get; private set; }
    public User Owner { get; private set; } = null!;

    public IReadOnlyCollection<Resource> Images => _images;

    public IReadOnlyCollection<Reaction> Reactions => _reactions;

    public IReadOnlyCollection<Comment> Comments => _comments;

    public static Result<Post> Create(string title, string description)
    {
        return new Post
        {
            Title = title,
            Description = description
        };
    }
}
