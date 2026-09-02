namespace Innovia.Api.Features.Auth.Login;

public static class AuthServiceExtensions
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth").WithTags("Auth");

        Login.Endpoint.Map(group);
        Logout.Endpoint.Map(group);

        return group;
    }
}