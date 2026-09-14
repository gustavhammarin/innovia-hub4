using Innovia.Api.Common.Result;

namespace Innovia.Api.Features.Availability.ManageAvailabilityRules.ListAvailabilityRules;

public static class Endpoint
{
    public static RouteHandlerBuilder Map(IEndpointRouteBuilder app)
    {
        return app.MapGet("/resource-types/{resourceTypeId:guid}", async (
            Guid resourceTypeId,
            Handler handler,
            Validator validator,
            CancellationToken ct) =>
        {
            var command = new Command(resourceTypeId);

            var validation = validator.Validate(command);
            if (!validation.IsValid)
                return validation.ToProblemResult();

            var result = await handler.HandleAsync(command, ct);
            return result.ToHttpResponse();
        });
    }
}
