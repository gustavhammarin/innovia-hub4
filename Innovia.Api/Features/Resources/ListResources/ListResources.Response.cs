namespace Innovia.Api.Features.Resources.ListResources;

public sealed record Response(
    Guid Id,
    string Name,
    string Description,
    Guid ResourceTypeId,
    DateTimeOffset CreatedAt
);
