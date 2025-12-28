using Ardalis.Result;
using Polchan.Core.Posts.Entities;
using Polchan.Core.Users.Entities;

namespace Polchan.Core.Threads.Entities;

public class Thread : BaseEntity
{
    private readonly List<Post> _posts = [];

    private Thread() { }

    public string Name { get; private set; } = string.Empty;

    public IReadOnlyCollection<Post> Posts => _posts;
    
    public Guid OwnerId { get; private set; }
    public User Owner { get; private set; } = null!;

    public static Result<Thread> Create(string name, User owner)
    {
        return new Thread
        {
            Name = name,
            Owner = owner
        };
    }
}
