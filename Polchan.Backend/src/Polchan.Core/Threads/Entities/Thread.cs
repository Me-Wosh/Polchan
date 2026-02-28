using Ardalis.Result;
using Polchan.Core.Posts.Entities;
using Polchan.Core.Threads.Enums;
using Polchan.Core.Users.Entities;

namespace Polchan.Core.Threads.Entities;

public class Thread : BaseEntity
{
    private readonly List<Post> _posts = [];
    private readonly List<User> _subscribers = [];

    private Thread() { }

    public string Name { get; private set; } = string.Empty;
    public ThreadCategory Category { get; private set; }

    public IReadOnlyCollection<Post> Posts => _posts;
    public IReadOnlyCollection<User> Subscribers => _subscribers;
    
    public Guid OwnerId { get; private set; }
    public User Owner { get; private set; } = null!;

    public static Result<Thread> Create(string name, User owner, ThreadCategory category)
    {
        var thread = new Thread();

        return Result.Success()
            .Bind(_ => thread.UpdateName(name))
            .Bind(_ => thread.UpdateOwner(owner))
            .Bind(_ => thread.UpdateCategory(category));
    }

    public Result<Thread> UpdateName(string name)
    {
        name = name.Trim();

        if (string.IsNullOrEmpty(name))
            return Result.Invalid(new ValidationError("Thread name cannot be empty"));

        Name = name;
        return this;
    }

    public Result<Thread> UpdateCategory(ThreadCategory category)
    {
        Category = category;
        return this;
    }

    private Result<Thread> UpdateOwner(User owner)
    {
        Owner = owner;
        return this;
    }
}
