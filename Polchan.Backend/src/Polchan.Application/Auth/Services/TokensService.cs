using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Ardalis.Result;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Polchan.Core.Users.Entities;
using Polchan.Shared.Options;

namespace Polchan.Application.Auth.Services;

public interface ITokensService
{
    Result<string> CreateJwt(User user);
    Result<string> CreateRefreshToken();
}

public class TokensService(IOptions<JwtOptions> jwtOptions) : ITokensService
{
    public Result<string> CreateJwt(User user)
    {
        var claims = new List<Claim>
        {
            new (JwtRegisteredClaimNames.Sub, user.Id.ToString()), // subject (the user)
            new (JwtRegisteredClaimNames.Email, user.Email),
            new ("role", user.UserRole.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Value.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);
        
        var tokenDescriptor = new JwtSecurityToken(
            issuer: jwtOptions.Value.Issuer,
            audience: jwtOptions.Value.Audience,
            expires: DateTime.UtcNow.AddMinutes(jwtOptions.Value.ExpirationInMinutes),
            claims: claims,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
    }

    public Result<string> CreateRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(256 / 8);
        var token = Convert.ToHexString(randomBytes);
        return token;
    }
}
