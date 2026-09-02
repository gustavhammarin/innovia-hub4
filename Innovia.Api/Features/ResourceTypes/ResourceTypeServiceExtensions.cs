using Innovia.Api.Common.Auth;

namespace Innovia.Api.Features.ResourceTypes;

public static class ResourceTypeServiceExtensions
{
    public static IServiceCollection AddResourceTypesFeature(this IServiceCollection services)
    {
        services.AddScoped<CreateResourceType.Handler>();
        services.AddScoped<CreateResourceType.Validator>();

        return services;
    }

    public static IEndpointRouteBuilder MapResourceTypesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/resource-types").WithTags("ResourceTypes");

        CreateResourceType.Endpoint.Map(group)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly);

        return group;
    }
}
