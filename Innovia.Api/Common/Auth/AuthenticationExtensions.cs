using Innovia.Api.Common.Auth.Jwt;
using Innovia.Api.Common.Database.Seed;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace Innovia.Api.Common.Auth;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddAppAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtAuthentication(configuration);

        services.AddAuthorizationBuilder()
            .AddPolicy(AuthorizationPolicies.AdminOnly, policy =>
                policy.RequireRole(Roles.Admin))
            .AddPolicy(AuthorizationPolicies.MemberOnly, policy =>
                policy.RequireRole(Roles.Member))
            .AddPolicy(AuthorizationPolicies.MemberOrAdmin, policy =>
                policy.RequireRole(Roles.Member, Roles.Admin))
            .SetFallbackPolicy(new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build());

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<RoleSeeder>();
        services.AddScoped<AdminUserSeeder>();
        services.AddScoped<AvailabilityRuleSeeder>();

        return services;
    }
}
