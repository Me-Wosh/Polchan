using Ardalis.Result;
using Polchan.Core.Resources.Entities;
using Polchan.Core.Users.Entities;
using Thread = Polchan.Core.Threads.Entities.Thread;

namespace Polchan.Core.Posts.Entities;

public class Post : BaseEntity
{
    private const int MaxImages = 5;

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

    public static Result<Post> Create(string title, string description, Guid threadId)
    {
        var post = new Post();

        return Result.Success()
            .Bind(_ => post.UpdateTitle(title))
            .Bind(_ => post.UpdateDescription(description))
            .Bind(_ => post.UpdateThreadId(threadId));
    }

    public Result<Post> UpdateTitle(string title)
    {
        title = title.Trim();

        var errors = new List<ValidationError>();

        if (string.IsNullOrEmpty(title))
            errors.Add(new ValidationError("Title cannot be empty"));

        if (title.Length > 100)
            errors.Add(new ValidationError("Title cannot exceed 100 characters"));

        if (errors.Count > 0)
            return Result.Invalid(errors);

        Title = title;
        return Result.Success(this);
    }

    public Result<Post> UpdateDescription(string description)
    {
        description = description.Trim();

        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(description))
            errors.Add(new ValidationError("Description cannot be empty"));

        if (description.Length > 255)
            errors.Add(new ValidationError("Description cannot exceed 255 characters"));

        if (errors.Count > 0)
            return Result.Invalid(errors);

        Description = description;
        return Result.Success(this);
    }

    public Result AddImages(List<Resource> images)
    {
        if (images.Count + _images.Count > MaxImages)
            return Result.Invalid(new ValidationError($"A post cannot have more than {MaxImages} images"));

        _images.AddRange(images);
        return Result.Success();
    }

    public Result RemoveImages(IEnumerable<Guid> imageIds)
    {
        _images.RemoveAll(i => imageIds.Contains(i.Id));
        return Result.Success();
    }

    private Result<Post> UpdateThreadId(Guid threadId)
    {
        if (threadId == Guid.Empty)
            return Result.Invalid(new ValidationError("ThreadId cannot be empty"));

        ThreadId = threadId;
        return Result.Success(this);
    }
}
