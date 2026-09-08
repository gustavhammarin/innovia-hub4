using Innovia.Api.Common.Result;

namespace Innovia.Api.Features.Users.ListUsers;

public static class Endpoint
{
    public static RouteHandlerBuilder Map(IEndpointRouteBuilder app)
    {
        return app.MapGet("/", async (
            string? search,
            Handler handler,
            CancellationToken ct) =>
        {
            var command = new Command(search);
            var result = await handler.HandleAsync(command, ct);
            return result.ToHttpResponse();
        });
    }
}
