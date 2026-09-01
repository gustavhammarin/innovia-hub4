using Innovia.Api.Common.Auth;
using Innovia.Api.Common.Result;

namespace Innovia.Api.Features.Bookings.CancelBooking;
public static class Endpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapDelete("/{bookingId:guid}", async (
            Guid bookingId,
            Handler handler,
            Validator validator,
            ICurrentUser currentUser,
            CancellationToken ct
        ) =>
        {
            if(currentUser.UserId is null)
                return Results.Unauthorized();

            var command = new Command(
                bookingId,
                currentUser.UserId.Value
            );

            var validation = validator.Validate(command);
            if (!validation.IsValid)
                return validation.ToProblemResult();
            
            var result = await handler.HandleAsync(command, ct);
            return result.ToHttpResponse();
        });
    }
}