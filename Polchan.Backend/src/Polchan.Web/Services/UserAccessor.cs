using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Ardalis.Result;
using Polchan.Application.Auth.Services;
using Polchan.Core.Users.Entities;

namespace Polchan.Web.Services;

public class UserAccessor(IHttpContextAccessor httpContextAccessor, ILogger<UserAccessor> logger) : IUserAccessor
{
    public Result<Guid> GetUserId()
    {
        logger.LogInformation("Retrieving user id from token");

        if (httpContextAccessor.HttpContext is null)
        {
            logger.LogError("Couldn't process the request: HttpContext is null");
            return Result.Error("Couldn't process the request");
        }

        var userId = httpContextAccessor.HttpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (userId is null)
        {
            var claimType = $"{nameof(JwtRegisteredClaimNames)}.{nameof(JwtRegisteredClaimNames.Sub)}";
            logger.LogError("Claim type {ClaimType} not found", claimType);
            return Result.Error("User id not found");
        }

        try
        {
            var parsedId = Guid.Parse(userId);
            logger.LogInformation("User id successfully parsed");
            return Result.Success(parsedId);
        }

        catch (Exception exception)
        {
            logger.LogError(exception, "Error while parsing user id: {userId}", userId);
            return Result.Error("Error while parsing user id");
        }
    }

    public Result<string> GetRefreshToken()
    {
        logger.LogInformation("Retrieving user refresh token from cookie");

        if (httpContextAccessor.HttpContext is null)
        {
            logger.LogError("Couldn't process the request: HttpContext is null");
            return Result.Error("Couldn't process the request");
        }

        var cookieName = "refreshToken";
        var refreshToken = httpContextAccessor.HttpContext.Request.Cookies[cookieName];

        if (refreshToken is null)
        {
            logger.LogError("Refresh token cookie '{CookieName}' not found", cookieName);
            return Result.Error("Refresh token cookie not found");
        }

        return Result.Success(refreshToken);
    }

    public Result SetRefreshToken(RefreshToken refreshToken)
    {
        logger.LogInformation("Setting user refresh token in cookie");

        if (httpContextAccessor.HttpContext is null)
        {
            logger.LogError("Couldn't process the request: HttpContext is null");
            return Result.Error("Couldn't process the request");
        }

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = refreshToken.ExpirationDate
        };

        httpContextAccessor.HttpContext.Response.Cookies.Append("refreshToken", refreshToken.Token, cookieOptions);
        logger.LogInformation("User refresh token set successfully");
        return Result.Success();
    }
}
