namespace Innovia.Api.Common.Database.Seed;

public static class SeedServiceExtensions
{
    public static IServiceCollection AddSeeders(this IServiceCollection services)
    {
        services.AddScoped<RoleSeeder>();
        services.AddScoped<AdminUserSeeder>();
        services.AddScoped<ResourceTypeSeeder>();
        services.AddScoped<ResourceSeeder>();
        services.AddScoped<AvailabilityRuleSeeder>();

        return services;
    }
}
