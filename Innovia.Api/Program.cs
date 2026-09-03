using Innovia.Api.Common.Auth;
using Innovia.Api.Common.Database;
using Innovia.Api.Common.Database.Entities;
using Innovia.Api.Common.Database.Seed;
using Innovia.Api.Common.OpenApi;
using Innovia.Api.Features.Auth;
using Innovia.Api.Features.Availability;
using Innovia.Api.Features.Bookings;
using Innovia.Api.Features.Resources;
using Innovia.Api.Features.ResourceTypes;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

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

builder.Services.AddProblemDetails();

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
    options.CreateSchemaReferenceId = jsonTypeInfo =>
    {
        var type = jsonTypeInfo.Type;
        var defaultId = Microsoft.AspNetCore.OpenApi.OpenApiOptions.CreateDefaultSchemaReferenceId(jsonTypeInfo);
        return type.FullName?.StartsWith("Innovia.Api.", StringComparison.Ordinal) == true
            ? type.FullName.Replace("Innovia.Api.", "").Replace('.', '_').Replace('+', '_')
            : defaultId;
    };
});

builder.Services.AddAppAuthentication(builder.Configuration);
builder.Services.AddSeeders();

builder.Services.AddAuthFeatures();
builder.Services.AddBookingsFeature();
builder.Services.AddResourcesFeature();
builder.Services.AddResourceTypesFeature();
builder.Services.AddAvailabilityFeature();

var app = builder.Build();

await app.ApplyMigrationsAsync();
await app.SeedAppDataAsync();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();
    app.MapScalarApiReference().AllowAnonymous();
}

app.UseStatusCodePages();

app.UseAuthentication();
app.UseAuthorization();


app.MapAuthEndpoints();
app.MapBookingsEndpoints();
app.MapResourcesEndpoints();
app.MapResourceTypesEndpoints();
app.MapAvailabilityEndpoints();


app.Run();

public partial class Program { }
