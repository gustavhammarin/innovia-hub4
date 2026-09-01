using Innovia.Api.Common.Auth;
using Innovia.Api.Common.Result;

namespace Innovia.Api.Features.Bookings.CreateBooking;

public static class Endpoint
{
    public static RouteHandlerBuilder Map(IEndpointRouteBuilder app)
    {
        return app.MapPost("/", async (
            Request request,
            Handler handler,
            Validator validator,
            ICurrentUser currentUser,
            CancellationToken ct
        ) =>
        {
            var command = new Command(
                request.ResourceId,
                currentUser.UserId!.Value,
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