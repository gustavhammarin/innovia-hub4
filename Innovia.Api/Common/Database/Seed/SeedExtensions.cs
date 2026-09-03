namespace Innovia.Api.Common.Database.Seed;

public static class SeedExtensions
{
    public static async Task SeedAppDataAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;

        var roleSeeder = services.GetRequiredService<RoleSeeder>();
        await roleSeeder.SeedAsync();

        var adminSeeder = services.GetRequiredService<AdminUserSeeder>();
        await adminSeeder.SeedAsync();

        var resourceTypeSeeder = services.GetRequiredService<ResourceTypeSeeder>();
        await resourceTypeSeeder.SeedAsync();

        var resourceSeeder = services.GetRequiredService<ResourceSeeder>();
        await resourceSeeder.SeedAsync();

        var availabilityRuleSeeder = services.GetRequiredService<AvailabilityRuleSeeder>();
        await availabilityRuleSeeder.SeedAsync();
    }
}
