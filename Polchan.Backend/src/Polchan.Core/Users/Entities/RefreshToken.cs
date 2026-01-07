using Ardalis.Result;

namespace Polchan.Core.Users.Entities;

public class RefreshToken : BaseEntity
{
    private RefreshToken() { }    

    public string Token { get; private set; } = string.Empty;
    public DateTime ExpirationDate { get; private set; }

    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    public static Result<RefreshToken> Create(string token, DateTime expirationDate)
    {
        var validationResult = Validate(token, expirationDate);

        if (!validationResult.IsSuccess)
            return validationResult.Map();

        return new RefreshToken
        {
            Token = token,
            ExpirationDate = expirationDate
        };
    }

    public Result<RefreshToken> Update(string newToken, DateTime newExpirationDate)
    {
        var validationResult = Validate(newToken, newExpirationDate);

        if (!validationResult.IsSuccess)
            return validationResult.Map();

        Token = newToken;
        ExpirationDate = newExpirationDate;

        return Result.Success(this);
    }

    private static Result Validate(string token, DateTime expirationDate)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(token))
            errors.Add(new ValidationError("Refresh token cannot be empty"));

        if (expirationDate <= DateTime.UtcNow)
            errors.Add(new ValidationError("Refresh token expiration date cannot be older than today's date"));

        return errors.Count == 0
            ? Result.Success()
            : Result.Invalid(errors);
    }
}
