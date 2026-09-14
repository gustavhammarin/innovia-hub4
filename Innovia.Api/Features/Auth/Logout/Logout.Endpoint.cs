using Innovia.Api.Common.Auth.Cookie;

namespace Innovia.Api.Features.Auth.Logout;

public static class Endpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/logout", async (
            HttpContext httpContext,
            Handler handler,
            CancellationToken ct
            ) =>
        {
            var rawToken = httpContext.Request.Cookies[AuthCookieNames.RefreshToken];
            if (!string.IsNullOrEmpty(rawToken))
                await handler.HandleAsync(rawToken, ct);

            
                
            httpContext.Response.Cookies.Delete(AuthCookieNames.AccessToken);
            httpContext.Response.Cookies.Delete(AuthCookieNames.RefreshToken, new CookieOptions {Path = "/auth"});
            return Results.Ok();
        }).AllowAnonymous();
    }
}