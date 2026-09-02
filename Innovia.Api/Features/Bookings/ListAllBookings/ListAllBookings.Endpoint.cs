
using Innovia.Api.Common.Auth;
using Innovia.Api.Common.Result;

namespace Innovia.Api.Features.Bookings.ListAllBookings
{
    public static class Endpoint
    {
        public static void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/", async (
                Handler handler,
                ICurrentUser currentUser,
                CancellationToken ct) =>
            {

                var command = new Command();
                var result = await handler.HandleAsync(command, ct);
                return result.ToHttpResponse();
            })
            .RequireAuthorization(AuthorizationPolicies.AdminOnly);
        }
        
    }
}