using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Ardalis.Result;
using Polchan.Application.Auth.Services;
using Polchan.Core.Users.Entities;

namespace Polchan.Web.Services;

public class UserAccessor(IHttpContextAccessor httpContextAccessor, ILogger<UserAccessor> logger) : IUserAccessor
{
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
