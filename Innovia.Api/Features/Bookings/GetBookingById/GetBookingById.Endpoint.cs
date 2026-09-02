using Innovia.Api.Common.Auth;
using Innovia.Api.Common.Result;

namespace Innovia.Api.Features.Bookings.GetBookingById;

public static class Endpoint
{
    public static void Map (IEndpointRouteBuilder app)
    {
        app.MapGet("/{id:guid}", async (
            Guid id, 
            Handler handler, 
            Validator validator,
            ICurrentUser currentUser,
            CancellationToken ct) =>
        {
            if (currentUser.UserId is null)
                return Results.Unauthorized();

                var command = new Command (id);

                var validation = validator.Validate(command);
                if(!validation.IsValid)
                return validation.ToProblemResult();

                var result = await handler.HandleAsync(command, ct);
                return result.ToHttpResponse();
        }
        );
    }
}