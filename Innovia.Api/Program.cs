using Innovia.Api.Common.Auth;
using Innovia.Api.Common.Database;
using Innovia.Api.Common.Database.Entities;
using Innovia.Api.Features.Auth;
using Innovia.Api.Features.Availability;
using Innovia.Api.Features.Bookings;
using Innovia.Api.Features.Resources;
using Innovia.Api.Features.ResourceTypes;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.Password.RequiredLength = 8;
})
.AddRoles<ApplicationRole>()
.AddEntityFrameworkStores<AppDbContext>()
.AddSignInManager()
.AddDefaultTokenProviders();

builder.Services.AddAppAuthentication(builder.Configuration);

builder.Services.AddBookingsFeature();
builder.Services.AddResourcesFeature();
builder.Services.AddResourceTypesFeature();
builder.Services.AddAvailabilityFeature();

var app = builder.Build();

await app.ApplyMigrationsAsync();
await app.SeedAppDataAsync();

app.UseAuthentication();
app.UseAuthorization();

app.MapBookingsEndpoints();
app.MapResourcesEndpoints();
app.MapResourceTypesEndpoints();
app.MapAvailabilityEndpoints();

app.MapAuthEndpoints();

app.Run();
