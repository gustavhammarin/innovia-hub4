using Innovia.Api.Common.Auth;

namespace Innovia.Api.Features.Users;

public static class UserServiceExtensions
{
    public static IServiceCollection AddUsersFeature(this IServiceCollection services)
    {
        services.AddScoped<ListUsers.Handler>();

        return services;
    }

    public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/admin/users")
            .WithTags("Users")
            .RequireAuthorization(AuthorizationPolicies.AdminOnly);

        ListUsers.Endpoint.Map(group);
        CreateUser.Endpoint.Map(group);
        UpdateUser.Endpoint.Map(group);
        DeleteUser.Endpoint.Map(group);

        return group;
    }
}
