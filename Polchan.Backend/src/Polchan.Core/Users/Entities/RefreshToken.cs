using Ardalis.Result;

namespace Polchan.Core.Users.Entities;

public class RefreshToken : BaseEntity
{
    private const int ExpirationInDays = 7;

    private RefreshToken() { }    

    public string Token { get; private set; } = string.Empty;
    public DateTime ExpirationDate { get; private set; }

    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    public static Result<RefreshToken> Create(string token)
    {
        var refreshToken = new RefreshToken();

        return Result.Success()
            .Bind(_ => refreshToken.Update(token));
    }

    public Result<RefreshToken> Update(string newToken)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(newToken))
            errors.Add(new ValidationError("Refresh token cannot be empty"));

        if (newToken == Token)
            errors.Add(new ValidationError("New refresh token must be different from the current one"));

        if (errors.Count > 0)
            return Result.Invalid(errors);

        Token = newToken;
        ExpirationDate = DateTime.UtcNow.AddDays(ExpirationInDays);

        return Result.Success(this);
    }
}
