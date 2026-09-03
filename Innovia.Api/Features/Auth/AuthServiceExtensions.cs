using Innovia.Api.Features.Auth;

namespace Innovia.Api.Features.Auth;

public static class AuthServiceExtensions
{
    public static IServiceCollection AddAuthFeature(this IServiceCollection services)
    {
        services.AddScoped<Login.Handler>();
        services.AddScoped<Login.Validator>();
        services.AddScoped<Register.Handler>();
        services.AddScoped<Register.Validator>();

        return services;
    }

    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth").WithTags("Auth");

        Login.Endpoint.Map(group);
        Logout.Endpoint.Map(group);
        Register.Endpoint.Map(group);

        return group;
    }
}
