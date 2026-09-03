using Innovia.Api.Common.Result;
using Innovia.Api.Features.Bookings;

namespace Innovia.Api.Features.Resources.ListResources;

public static class Endpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/", async(
            Handler handler,
            CancellationToken ct
        ) =>
        {
            var query = new Query();
            var result = await handler.HandleAsync(query, ct);
            return result.ToHttpResponse();
        });
    }

}