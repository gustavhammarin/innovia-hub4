using Innovia.Api.Common.Auth.Cookie;
using Innovia.Api.Common.Auth.Cookies;
using Innovia.Api.Common.Auth.Jwt;
using Innovia.Api.Common.Errors;
using Innovia.Api.Common.Result;
using Microsoft.Extensions.Options;

namespace Innovia.Api.Features.Auth.Login;

public static class Endpoint
{
    public static RouteHandlerBuilder Map(IEndpointRouteBuilder app)
    {
        return app.MapPost("/login", async (
            Command cmd,
            HttpContext httpContext,
            Handler handler,
            Validator validator,
            IOptions<JwtSettings> jwtSettings,
            CancellationToken ct
        ) =>
        {
            var validation = validator.Validate(cmd);
            if (!validation.IsValid)
                return validation.ToProblemResult();

            var result = await handler.HandleAsync(cmd, ct);

            if (!result.IsSuccess)
                return result.Error!.ToProblemResult();

            httpContext.Response.Cookies.Append(
                AuthCookieNames.AccessToken,
                result.Value!,
                CookieOptionsFactory.CreateAccessTokenCookieOptions(
                    DateTimeOffset.UtcNow.AddMinutes(jwtSettings.Value.AccessTokenExpirationMinutes)
                )
            );

            return Results.Ok();
        })
        .AllowAnonymous();
    }
}