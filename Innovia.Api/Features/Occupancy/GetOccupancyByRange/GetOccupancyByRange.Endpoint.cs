using Innovia.Api.Common.Result;

namespace Innovia.Api.Features.Occupancy.GetOccupancyByRange;

public static class Endpoint
{
    public static RouteHandlerBuilder Map(IEndpointRouteBuilder app)
    {
        return app.MapGet("/range", async (
            DateOnly from,
            DateOnly to,
            Handler handler,
            CancellationToken ct
        ) =>
        {
            var command = new Command(from, to);
            var result = await handler.HandleAsync(command, ct);
            return result.ToHttpResponse();
        });
    }
}