using Innovia.Api.Common.Result;

namespace Innovia.Api.Features.Availability.ManageAvailabilityRules.UpdateAvailabilityRule;

public static class Endpoint
{
    public static RouteHandlerBuilder Map(IEndpointRouteBuilder app)
    {
        return app.MapPut("/{id:guid}", async (
            Guid id,
            Request request,
            Handler handler,
            Validator validator,
            CancellationToken ct) =>
        {
            var command = new Command(
                id,
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
