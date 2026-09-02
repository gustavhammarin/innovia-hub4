using Innovia.Api.Common.Auth;

namespace Innovia.Api.Features.Resources;

public static class ResourceServiceExtensions
{
    public static IServiceCollection AddResourcesFeature(this IServiceCollection services)
    {
        services.AddScoped<CreateResource.Handler>();
        services.AddScoped<CreateResource.Validator>();

        return services;
    }

    public static IEndpointRouteBuilder MapResourcesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/resources").WithTags("Resources");

        CreateResource.Endpoint.Map(group)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly);

        return group;
    }
}
