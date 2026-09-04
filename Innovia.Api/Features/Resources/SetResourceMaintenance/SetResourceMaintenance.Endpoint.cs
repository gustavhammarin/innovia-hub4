using Innovia.Api.Common.Result;

namespace Innovia.Api.Features.Resources.SetResourceMaintenance;

public static class Endpoint
{
    public static RouteHandlerBuilder Map(IEndpointRouteBuilder app)
    {
        return app.MapPost("/{id:guid}/maintenance", async (
            Guid id,
            Handler handler,
            Validator validator,
            CancellationToken ct
        ) =>
        {
            var command = new Command(id);

            var validation = validator.Validate(command);
            if (!validation.IsValid)
                return validation.ToProblemResult();

            var result = await handler.HandleAsync(command, ct);
            return result.ToHttpResponse();
        });
    }
}
