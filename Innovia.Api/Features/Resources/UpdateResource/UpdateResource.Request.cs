namespace Innovia.Api.Features.Resources.UpdateResource;

public sealed record Request(
    string Name,
    string Description,
    Guid ResourceTypeId
);
