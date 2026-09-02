using Innovia.Api.Common.Result;

namespace Innovia.Api.Features.Resources.GetResourceById;

public static class Endpoint
{
    public static RouteHandlerBuilder Map(IEndpointRouteBuilder app)
    {
        return app.MapGet("/{id:guid}", async (
            Guid id,
            Handler handler,
            Validator validator,
            CancellationToken ct) =>
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
