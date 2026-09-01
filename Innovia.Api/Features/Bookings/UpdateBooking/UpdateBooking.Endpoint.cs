using Innovia.Api.Common.Auth;
using Innovia.Api.Common.Result;

namespace Innovia.Api.Features.Bookings.UpdateBooking;

public static class Endpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut("/{bookingId:guid}", async (
            Guid bookingId,
            Request request,
            Handler handler,
            Validator validator,
            ICurrentUser currentUser,
            CancellationToken ct
        ) =>
        {
            if (currentUser.UserId is null)
                return Results.Unauthorized();

            var command = new Command(
                bookingId,
                request.ResourceId,
                currentUser.UserId.Value,
                request.StartsAt,
                request.EndsAt
            );

            var validation = validator.Validate(command);
            if (!validation.IsValid)
                return validation.ToProblemResult();

            var result = await handler.HandleAsync(command, ct);
            return result.ToHttpResponse();
        });
    }
}
