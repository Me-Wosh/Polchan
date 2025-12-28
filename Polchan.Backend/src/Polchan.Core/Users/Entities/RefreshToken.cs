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
        return new RefreshToken
        {
            Token = token,
            ExpirationDate = expirationDate
        };
    }
}
