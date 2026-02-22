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
        var validationResult = Validate(title, description, threadId);

        if (!validationResult.IsSuccess)
            return validationResult.Map();

        return new Post
        {
            Title = title,
            Description = description,
            ThreadId = threadId
        };
    }

    public Result<Post> UpdateTitle(string title)
    {
        return Result.Success()
            .Bind(_ => ValidateTitle(title))
            .Bind(_ =>
            {
                Title = title;
                return Result.Success(this);
            });
    }

    public Result<Post> UpdateDescription(string description)
    {
        return Result.Success()
            .Bind(_ => ValidateDescription(description))
            .Bind(_ =>
            {
                Description = description;
                return Result.Success(this);
            });
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

    private static Result Validate(string title, string description, Guid threadId)
    {
        var errors = new List<ValidationError>();

        errors.AddRange(ValidateTitle(title).ValidationErrors);
        errors.AddRange(ValidateDescription(description).ValidationErrors);
        errors.AddRange(ValidateThreadId(threadId).ValidationErrors);

        return errors.Count == 0
            ? Result.Success()
            : Result.Invalid(errors);
    }

    private static Result ValidateTitle(string title)
    {
        title = title.Trim();

        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(title))
            errors.Add(new ValidationError("Title cannot be empty"));

        if (title.Length > 100)
            errors.Add(new ValidationError("Title cannot exceed 100 characters"));

        return errors.Count == 0
            ? Result.Success()
            : Result.Invalid(errors);
    }

    private static Result ValidateDescription(string description)
    {
        description = description.Trim();

        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(description))
            errors.Add(new ValidationError("Description cannot be empty"));

        if (description.Length > 255)
            errors.Add(new ValidationError("Description cannot exceed 255 characters"));

        return errors.Count == 0
            ? Result.Success()
            : Result.Invalid(errors);
    }

    private static Result ValidateThreadId(Guid threadId)
    {
        if (threadId == Guid.Empty)
            return Result.Invalid(new ValidationError("ThreadId cannot be empty"));

        return Result.Success();
    }
}
