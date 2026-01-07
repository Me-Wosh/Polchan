namespace Polchan.Shared.Options;

public class JwtOptions
{
    public required string Secret { get; init; }
    public required string Issuer { get; init; }
    public required string Audience { get; init; }
    public required int ExpirationInMinutes { get; init; }
};
