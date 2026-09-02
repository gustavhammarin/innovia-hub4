namespace Innovia.Api.Features.Resources.CreateResource;

public sealed record Request(
    string Name,
    string Description,
    Guid ResourceTypeId
);
