using Innovia.Api.Common.Auth;

namespace Innovia.Api.Features.Resources;

public static class ResourceServiceExtensions
{
    public static IServiceCollection AddResourcesFeature(this IServiceCollection services)
    {
        services.AddScoped<CreateResource.Handler>();
        services.AddScoped<CreateResource.Validator>();

        services.AddScoped<UpdateResource.Handler>();
        services.AddScoped<UpdateResource.Validator>();

        services.AddScoped<ListResources.Handler>();

        services.AddScoped<ListResourcesAdmin.Handler>();
      
        services.AddScoped<GetResourceById.Handler>();
        services.AddScoped<GetResourceById.Validator>();

        services.AddScoped<GetResourceByIdAdmin.Handler>();
        services.AddScoped<GetResourceByIdAdmin.Validator>();

        services.AddScoped<DeleteResource.Handler>();
        services.AddScoped<DeleteResource.Validator>();

        services.AddScoped<UnarchiveResource.Handler>();
        services.AddScoped<UnarchiveResource.Validator>();

        services.AddScoped<SetResourceMaintenance.Handler>();
        services.AddScoped<SetResourceMaintenance.Validator>();

        services.AddScoped<SetResourceOnline.Handler>();
        services.AddScoped<SetResourceOnline.Validator>();

        services.AddScoped<SetResourceOffline.Handler>();
        services.AddScoped<SetResourceOffline.Validator>();

        return services;
    }

    public static IEndpointRouteBuilder MapResourcesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/resources")
            .WithTags("Resources")
            .AddEndpointFilter<RequireCurrentUserFilter>();

        CreateResource.Endpoint.Map(group)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly);
        UpdateResource.Endpoint.Map(group)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly);
        GetResourceById.Endpoint.Map(group)
            .RequireAuthorization(AuthorizationPolicies.MemberOrAdmin);
        ListResources.Endpoint.Map(group)
            .RequireAuthorization(AuthorizationPolicies.MemberOrAdmin);
        ListResourcesAdmin.Endpoint.Map(group)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly);
        GetResourceByIdAdmin.Endpoint.Map(group)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly);
        DeleteResource.Endpoint.Map(group)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly);
        UnarchiveResource.Endpoint.Map(group)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly);
        SetResourceMaintenance.Endpoint.Map(group)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly);
        SetResourceOnline.Endpoint.Map(group)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly);
        SetResourceOffline.Endpoint.Map(group)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly);

        return group; 
     
    }
}
