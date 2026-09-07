using Innovia.Api.Common.Auth;
using Innovia.Api.Common.Database.Entities;
using Microsoft.AspNetCore.Identity;

namespace Innovia.Api.Features.Auth.Me;

public static class Endpoint
{
    public static RouteHandlerBuilder Map(IEndpointRouteBuilder app)
    {
        return app.MapGet("/me", async (
            ICurrentUser currentUser,
            UserManager<ApplicationUser> userManager,
            CancellationToken ct
        ) =>
        {
            if (currentUser.UserId is null)
                return Results.Unauthorized();

            var user = await userManager.FindByIdAsync(currentUser.UserId.Value.ToString());
            if (user is null)
                return Results.Unauthorized();

            var roles = await userManager.GetRolesAsync(user);

            return Results.Ok(new Response(user.Id, user.Email ?? "Unknown", roles));
        });
    }
}
