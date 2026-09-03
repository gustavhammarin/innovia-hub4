using Innovia.Api.Common.Result;

namespace Innovia.Api.Features.Bookings.ListBookings;

public static class Endpoint
{
    public static RouteHandlerBuilder Map(IEndpointRouteBuilder app)
    {
        return app.MapGet("/", async (
            Guid? userId,
            Guid? resourceId,
            DateOnly? from,
            DateOnly? to,
            Handler handler,
            Validator validator,
            CancellationToken ct) =>
        {
            var command = new Command(userId, resourceId, from, to);

            var validation = validator.Validate(command);
            if (!validation.IsValid)
                return validation.ToProblemResult();

            var result = await handler.HandleAsync(command, ct);
            return result.ToHttpResponse();
        });
    }
}
