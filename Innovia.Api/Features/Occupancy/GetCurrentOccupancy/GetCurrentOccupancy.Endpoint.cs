using Innovia.Api.Common.Result;

namespace Innovia.Api.Features.Occupancy.GetCurrentOccupancy;

public static class Endpoint
{
    public static RouteHandlerBuilder Map(IEndpointRouteBuilder app)
    {
        return app.MapGet("/current", async (
            Handler handler,
            CancellationToken ct
        ) =>
        {
            var command = new Command();
            var result = await handler.HandleAsync(command, ct);
            return result.ToHttpResponse();
        });
    }
}