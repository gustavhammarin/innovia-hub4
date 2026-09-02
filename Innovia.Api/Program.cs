using Innovia.Api.Common.Auth;
using Innovia.Api.Common.Database;
using Innovia.Api.Common.Database.Entities;
using Innovia.Api.Features.Bookings;
using Innovia.Api.Features.Resources;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Innovia.Api.Features.Auth.Login;
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

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

builder.Services.AddBookingsFeature();
builder.Services.AddResourcesFeature();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapBookingsEndpoints();
app.MapResourcesEndpoints();

app.MapAuthEndpoints();

app.Run();
