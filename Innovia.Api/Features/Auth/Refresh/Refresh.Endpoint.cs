using Innovia.Api.Common.Auth.Cookie;
using Innovia.Api.Common.Auth.Cookies;
using Innovia.Api.Common.Auth.Jwt;
using Innovia.Api.Common.Errors;
using Microsoft.Extensions.Options;

namespace Innovia.Api.Features.Auth.Refresh;

public static class Endpoint
{
    public static RouteHandlerBuilder Map(IEndpointRouteBuilder app)
    {
        return app.MapPost("/refresh", async (
            HttpContext httpContext,
            Handler handler,
            IOptions<JwtSettings> jwtSettings,
            CancellationToken ct
        ) =>
        {
            var rawToken = httpContext.Request.Cookies[AuthCookieNames.RefreshToken];
            if (string.IsNullOrEmpty(rawToken))
                return AuthErrors.InvalidCredentials().ToProblemResult();
            
            var result = await handler.HandleAsync(new Command(rawToken), ct);

            if (!result.IsSuccess)
                return result.Error!.ToProblemResult();
            
            httpContext.Response.Cookies.Append(
                AuthCookieNames.AccessToken,
                result.Value!.AccessToken,
                CookieOptionsFactory.CreateAccessTokenCookieOptions(
                    DateTimeOffset.UtcNow.AddMinutes(jwtSettings.Value.AccessTokenExpirationMinutes)
                )
            );
            httpContext.Response.Cookies.Append(
                AuthCookieNames.RefreshToken,
                result.Value!.RefreshToken,
                CookieOptionsFactory.CreateRefreshTokenCookieOptions(
                    DateTimeOffset.UtcNow.AddDays(jwtSettings.Value.RefreshTokenExpirationDays)
                )
            );

            return Results.Ok();
        }).AllowAnonymous();
    }
}