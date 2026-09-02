namespace Innovia.Api.Common.Auth.Cookies;

public static class CookieOptionsFactory
{
    public static CookieOptions CreateAccessTokenCookieOptions(DateTimeOffset expires) => new()
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Strict,
        Expires = expires
    };
}