using Innovia.Api.Common.Result;

namespace Innovia.Api.Features.ResourceTypes.CreateResourceType;

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
            var command = new Command(request.Name, request.MaxDurationMinutes, request.MaxAdvanceDays);

            var validation = validator.Validate(command);
            if (!validation.IsValid)
                return validation.ToProblemResult();

            var result = await handler.HandleAsync(command, ct);
            return result.ToHttpResponse();
        });
    }
}
