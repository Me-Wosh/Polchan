using Ardalis.Result;
using Polchan.Core.Posts.Entities;
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

    public static Result<User> Create(
        string email,
        string username,
        string passwordHash,
        UserRole userRole
    )
    {
        return new User
        {
            Email = email,
            Username = username,
            PasswordHash = passwordHash,
            UserRole = userRole
        };
    }
}
