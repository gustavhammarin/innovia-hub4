namespace Innovia.Api.Features.Auth;

public static class AuthServiceExtensions
{
    public static IServiceCollection AddAuthFeatures(this IServiceCollection services)
    {
        services.AddScoped<Login.Handler>();
        services.AddScoped<Login.Validator>();
        services.AddScoped<Register.Handler>();
        services.AddScoped<Register.Validator>();
        services.AddScoped<Refresh.Handler>();
        services.AddScoped<Logout.Handler>();

        return services;
    }

    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth").WithTags("Auth");

        Login.Endpoint.Map(group);
        Logout.Endpoint.Map(group);
        Register.Endpoint.Map(group);
        Refresh.Endpoint.Map(group);

        return group;
    }
}
