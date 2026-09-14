namespace Innovia.Api.Features.Resources.UpdateResource;

public sealed record Command(
    Guid Id,
    string Name,
    string Description,
    Guid ResourceTypeId
);
