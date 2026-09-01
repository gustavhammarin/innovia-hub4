using Innovia.Api.Common.Result;

namespace Innovia.Api.Features.Resources.CreateResource;

public static class Endpoint
{
    public static RouteHandlerBuilder Map(IEndpointRouteBuilder app)
    {
        return app.MapPost("/", async (
            Request request,
            Handler handler,
            Validator validator,
            CancellationToken ct
        ) =>
        {
            var command = new Command(
                request.Name,
                request.Description,
                request.ResourceTypeId
            );

            var validation = validator.Validate(command);
            if (!validation.IsValid)
                return validation.ToProblemResult();

            var result = await handler.HandleAsync(command, ct);
            return result.ToHttpResponse();
        });
    }
}
