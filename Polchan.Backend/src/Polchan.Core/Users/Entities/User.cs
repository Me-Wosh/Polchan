using Ardalis.Result;
using Polchan.Core.Posts.Entities;
using Polchan.Core.Users.Enums;
using Thread = Polchan.Core.Threads.Entities.Thread;

namespace Polchan.Core.Users.Entities;

public class User : BaseEntity
{
    private const int RefreshTokenExpirationInDays = 7;

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
        var validationResult = Validate(email, username, passwordHash);

        if (!validationResult.IsSuccess)
            return validationResult.Map();

        return new User
        {
            Email = email,
            Username = username,
            PasswordHash = passwordHash,
            UserRole = userRole
        };
    }

    public Result<RefreshToken> AddRefreshToken(string token)
    {
        var expirationDate = DateTime.UtcNow.AddDays(RefreshTokenExpirationInDays);
        var refreshToken = RefreshToken.Create(token, expirationDate);

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

        var expirationDate = DateTime.UtcNow.AddDays(RefreshTokenExpirationInDays);
        return refreshToken.Update(newToken, expirationDate);
    }

    private static Result Validate(string email, string username, string passwordHash)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(email))
            errors.Add(new ValidationError("Email cannot be empty"));

        if (string.IsNullOrWhiteSpace(username))
            errors.Add(new ValidationError("Username cannot be empty"));

        if (string.IsNullOrWhiteSpace(passwordHash))
            errors.Add(new ValidationError("Password cannot be empty"));

        return errors.Count == 0
            ? Result.Success()
            : Result.Invalid(errors);
    }
}
