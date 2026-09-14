using Innovia.Api.Common.Auth;
using Innovia.Api.Common.Result;

namespace Innovia.Api.Features.Bookings.GetMyBookings;

public static class Endpoint
{
    public static RouteHandlerBuilder Map(IEndpointRouteBuilder app)
    {
        return app.MapGet("/me", async (
            Handler handler,
            ICurrentUser currentUser,
            CancellationToken ct) =>
        {
            var command = new Command(currentUser.UserId!.Value);
            var result = await handler.HandleAsync(command, ct);
            return result.ToHttpResponse();
        });
    }
}
