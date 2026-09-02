using Innovia.Api.Features.Auth.Login;

namespace Innovia.Api.Features.Auth;

public static class AuthServiceExtensions
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth").WithTags("Auth");

        Login.Endpoint.Map(group);

        return group;
    }
}
