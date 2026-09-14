using Innovia.Api.Common.Auth;

namespace Innovia.Api.Features.ResourceTypes;

public static class ResourceTypeServiceExtensions
{
    public static IServiceCollection AddResourceTypesFeature(this IServiceCollection services)
    {
        services.AddScoped<CreateResourceType.Handler>();
        services.AddScoped<CreateResourceType.Validator>();

        services.AddScoped<UpdateResourceType.Handler>();
        services.AddScoped<UpdateResourceType.Validator>();

        services.AddScoped<ListResourceTypes.Handler>();

        return services;
    }

    public static IEndpointRouteBuilder MapResourceTypesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/resource-types").WithTags("ResourceTypes");

        CreateResourceType.Endpoint.Map(group)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly);
        UpdateResourceType.Endpoint.Map(group)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly);
        ListResourceTypes.Endpoint.Map(group);

        return group;
    }
}
