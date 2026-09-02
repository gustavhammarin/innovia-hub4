using Innovia.Api.Common.Auth;
using Innovia.Api.Common.Result;

namespace Innovia.Api.Features.Bookings.UpdateBooking;

public static class Endpoint
{
    public static RouteHandlerBuilder Map(IEndpointRouteBuilder app)
    {
        return app.MapPut("/{bookingId:guid}", async (
            Guid bookingId,
            Request request,
            Handler handler,
            Validator validator,
            ICurrentUser currentUser,
            CancellationToken ct
        ) =>
        {
            var command = new Command(
                bookingId,
                request.ResourceId,
                currentUser.UserId!.Value,
                currentUser.IsAdmin,
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
