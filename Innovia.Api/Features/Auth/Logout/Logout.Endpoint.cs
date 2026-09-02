using Innovia.Api.Common.Auth.Cookie;

namespace Innovia.Api.Features.Auth.Logout;

public static class Endpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/logout", (HttpContext httpContext) =>
        {
            httpContext.Response.Cookies.Delete(AuthCookieNames.AccessToken);
            return Results.Ok();
        });
    }
}