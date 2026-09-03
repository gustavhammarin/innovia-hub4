using Innovia.Api.Common.Result;

namespace Innovia.Api.Features.Availability.ManageAvailabilityRules.CreateAvailabilityRule;

public static class Endpoint
{
    public static RouteHandlerBuilder Map(IEndpointRouteBuilder app)
    {
        return app.MapPost("/", async (
            Request request,
            Handler handler,
            Validator validator,
            CancellationToken ct) =>
        {
            var command = new Command(
                request.ResourceTypeId,
                request.DayOfWeek,
                request.OpensAt,
                request.ClosesAt,
                request.SlotDurationMinutes);

            var validation = validator.Validate(command);
            if (!validation.IsValid)
                return validation.ToProblemResult();

            var result = await handler.HandleAsync(command, ct);
            return result.ToHttpResponse();
        });
    }
}
