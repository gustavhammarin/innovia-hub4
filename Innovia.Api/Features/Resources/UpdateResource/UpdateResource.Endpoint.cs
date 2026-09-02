
using Innovia.Api.Common.Auth;
using Innovia.Api.Common.Result;

namespace Innovia.Api.Features.Resources.UpdateResource;

public static class Endpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut("/{id}:Guid", async (
            Guid id, 
            Handler handler,
            Request request, 
            Validator validator, 
            CancellationToken ct) =>
        {
            var command = new Command (id, request.Name, request.ResourceTypeId);

            var validation = validator.Validate(command);
            if (!validation.IsValid)
                return validation.ToProblemResult();

            var result = await handler.HandleAsync(command, ct);
            return result.ToHttpResponse();
            
        })
        .RequireAuthorization(AuthorizationPolicies.AdminOnly);
        
    }
}