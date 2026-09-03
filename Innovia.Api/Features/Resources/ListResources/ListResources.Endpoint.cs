using Innovia.Api.Common.Result;

namespace Innovia.Api.Features.Resources.ListResources;

public static class Endpoint
{
    public static RouteHandlerBuilder Map(IEndpointRouteBuilder app)
    {
        return app.MapGet("/", async (
            Handler handler,
            CancellationToken ct
        ) =>
        {
            var result = await handler.HandleAsync(ct);
            return result.ToHttpResponse();
        });
    }
}
