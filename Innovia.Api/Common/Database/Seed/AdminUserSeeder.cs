using Innovia.Api.Common.Auth;
using Innovia.Api.Common.Database.Entities;
using Microsoft.AspNetCore.Identity;

namespace Innovia.Api.Common.Database.Seed;

public class AdminUserSeeder
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;

    public AdminUserSeeder(UserManager<ApplicationUser> userManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _configuration = configuration;
    }

    public async Task SeedAsync()
    {
        var adminEmail = _configuration["AdminUser:Email"];
        var adminPassword = _configuration["AdminUser:Password"];

        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
            return;

        var existingAdmin = await _userManager.FindByEmailAsync(adminEmail);
        if (existingAdmin is not null)
            return;

        var admin = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            FirstName = "Admin",
            LastName = "Admin"
        };

        var result = await _userManager.CreateAsync(admin, adminPassword);
        if (!result.Succeeded)
        {
            var message = string.Join(" ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to seed admin user: {message}");
        }

        await _userManager.AddToRoleAsync(admin, Roles.Admin);
    }
}
