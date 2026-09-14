using Innovia.Api.Common.Auth;
using Innovia.Api.Common.Database.Entities;
using Microsoft.AspNetCore.Identity;

namespace Innovia.Api.Common.Database.Seed;

public class RoleSeeder
{
    private readonly RoleManager<ApplicationRole> _roleManager;

    public RoleSeeder(RoleManager<ApplicationRole> roleManager)
    {
        _roleManager = roleManager;
    }

    public async Task SeedAsync()
    {
        foreach (var roleName in Roles.All)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new ApplicationRole {Name = roleName});
            }
        }
    }
}
