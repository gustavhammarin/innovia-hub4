using Innovia.Api.Common.Auth;

namespace Innovia.Api.Features.Availability;

public static class AvailabilityServiceExtensions
{
    public static IServiceCollection AddAvailabilityFeature(this IServiceCollection services)
    {
        services.AddScoped<GetResourceAvailability.Handler>();
        services.AddScoped<GetResourceAvailability.Validator>();

        services.AddScoped<ManageAvailabilityRules.ListAvailabilityRules.Handler>();
        services.AddScoped<ManageAvailabilityRules.ListAvailabilityRules.Validator>();

        services.AddScoped<ManageAvailabilityRules.CreateAvailabilityRule.Handler>();
        services.AddScoped<ManageAvailabilityRules.CreateAvailabilityRule.Validator>();

        services.AddScoped<ManageAvailabilityRules.UpdateAvailabilityRule.Handler>();
        services.AddScoped<ManageAvailabilityRules.UpdateAvailabilityRule.Validator>();

        services.AddScoped<ManageAvailabilityRules.DeleteAvailabilityRule.Handler>();
        services.AddScoped<ManageAvailabilityRules.DeleteAvailabilityRule.Validator>();

        return services;
    }

    public static IEndpointRouteBuilder MapAvailabilityEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/availability").WithTags("Availability");

        GetResourceAvailability.Endpoint.Map(group)
            .RequireAuthorization(AuthorizationPolicies.MemberOrAdmin);

        var ruleGroup = group.MapGroup("/rules");
        ManageAvailabilityRules.ListAvailabilityRules.Endpoint.Map(ruleGroup)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly);
        ManageAvailabilityRules.CreateAvailabilityRule.Endpoint.Map(ruleGroup)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly);
        ManageAvailabilityRules.UpdateAvailabilityRule.Endpoint.Map(ruleGroup)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly);
        ManageAvailabilityRules.DeleteAvailabilityRule.Endpoint.Map(ruleGroup)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly);

        return group;
    }
}
