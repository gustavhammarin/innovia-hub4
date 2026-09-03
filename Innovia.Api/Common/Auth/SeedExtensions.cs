using Innovia.Api.Common.Database.Seed;

namespace Innovia.Api.Common.Auth;

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

        var availabilityRuleSeeder = services.GetRequiredService<AvailabilityRuleSeeder>();
        await availabilityRuleSeeder.SeedAsync();
    }
}
