using Innovia.Api.Common.Result;

namespace Innovia.Api.Features.Resources.ListResourcesAdmin;

public static class Endpoint
{
    public static RouteHandlerBuilder Map(IEndpointRouteBuilder app)
    {
        return app.MapGet("/admin", async (
            Handler handler,
            CancellationToken ct
        ) =>
        {
            var result = await handler.HandleAsync(ct);
            return result.ToHttpResponse();
        });
    }
}
