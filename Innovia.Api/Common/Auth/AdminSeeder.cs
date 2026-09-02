using Innovia.Api.Common.Database.Entities;
using Microsoft.AspNetCore.Identity;

namespace Innovia.Api.Common.Auth;

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
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(admin, adminPassword);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(admin, Roles.Admin);
        }
    }
}