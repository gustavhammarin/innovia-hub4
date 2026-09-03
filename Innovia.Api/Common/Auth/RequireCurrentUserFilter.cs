namespace Innovia.Api.Common.Auth;

public sealed class RequireCurrentUserFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var currentUser = context.HttpContext.RequestServices.GetRequiredService<ICurrentUser>();

        if (currentUser.UserId is null)
            return Results.Unauthorized();

        return await next(context);
    }
}
