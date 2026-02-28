using Ardalis.Result;
using Polchan.Core.Posts.Entities;
using Polchan.Core.Threads.Enums;
using Polchan.Core.Users.Enums;
using Thread = Polchan.Core.Threads.Entities.Thread;

namespace Polchan.Core.Users.Entities;

public class User : BaseEntity
{
    private readonly List<Post> _posts = [];
    private readonly List<Comment> _comments = [];
    private readonly List<Reaction> _reactions = [];
    private readonly List<RefreshToken> _refreshTokens = [];
    private readonly List<Thread> _ownedThreads = [];
    private readonly List<Thread> _subscribedThreads = [];

    private User() { }

    public string Email { get; private set; } = string.Empty;
    public string Username { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public UserRole UserRole { get; private set; } = UserRole.User;

    public IReadOnlyCollection<Post> Posts => _posts;

    public IReadOnlyCollection<Comment> Comments => _comments;
    
    public IReadOnlyCollection<Reaction> Reactions => _reactions;
    
    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens;

    public IReadOnlyCollection<Thread> OwnedThreads => _ownedThreads;
    public IReadOnlyCollection<Thread> SubscribedThreads => _subscribedThreads;

    public static Result<User> Create(string email, string username, string passwordHash, UserRole userRole)
    {
        var user = new User();

        return Result.Success()
            .Bind(_ => user.UpdateEmail(email))
            .Bind(_ => user.UpdateUsername(username))
            .Bind(_ => user.UpdatePasswordHash(passwordHash))
            .Bind(_ => user.UpdateUserRole(userRole));
    }

    public Result<RefreshToken> AddRefreshToken(string token)
    {
        var refreshToken = RefreshToken.Create(token);

        if (!refreshToken.IsSuccess)
            return refreshToken.Map();

        _refreshTokens.Add(refreshToken);
        return Result.Success(refreshToken);
    }

    public Result<RefreshToken> ReplaceRefreshToken(Guid oldTokenId, string newToken)
    {
        var refreshToken = _refreshTokens.SingleOrDefault(rt => rt.Id == oldTokenId);
        
        if (refreshToken is null)
            return Result.Error("Old refresh token not found");

        return refreshToken.Update(newToken);
    }

    public Result AddOwnedThread(Thread thread)
    {
        if (_ownedThreads.Any(t => t.Id == thread.Id))
            return Result.Error($"User is already the owner of thread with id: '{thread.Id}'");

        _ownedThreads.Add(thread);
        return Result.Success();
    }

    public Result SubscribeThread(Thread thread)
    {
        if (_subscribedThreads.Any(t => t.Id == thread.Id))
            return Result.Error($"User already subscribes thread with id: '{thread.Id}'");

        _subscribedThreads.Add(thread);
        return Result.Success();
    }

    public Result UnsubscribeThread(Guid threadId)
    {
        var thread = _subscribedThreads.SingleOrDefault(t => t.Id == threadId);

        if (thread is null)
            return Result.Error($"User doesn't subscribe thread with id: '{threadId}'");

        _subscribedThreads.Remove(thread);
        return Result.Success();
    }

    public Result AddPost(Post post)
    {
        if (_posts.Any(p => p.Id == post.Id))
            return Result.Error($"User already added post with id: '{post.Id}'");

        _posts.Add(post);
        return Result.Success();
    }

    private Result<User> UpdateEmail(string email)
    {
        email = email.Trim();

        if (string.IsNullOrEmpty(email))
            return Result.Invalid(new ValidationError("Email cannot be empty"));

        Email = email;
        return Result.Success(this);
    }

    private Result<User> UpdateUsername(string username)
    {
        username = username.Trim();

        if (string.IsNullOrEmpty(username))
            return Result.Invalid(new ValidationError("Username cannot be empty"));

        Username = username;
        return Result.Success(this);
    }

    private Result<User> UpdatePasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            return Result.Invalid(new ValidationError("Password cannot be empty"));

        PasswordHash = passwordHash;
        return Result.Success(this);
    }

    private Result<User> UpdateUserRole(UserRole userRole)
    {
        UserRole = userRole;
        return Result.Success(this);
    }
}
