namespace Innovia.Api.Features.Auth.Me;

public sealed record Response(
    Guid Id,
    string Email,
    IList<string> Roles
);
