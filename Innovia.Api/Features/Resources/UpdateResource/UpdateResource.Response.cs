using Innovia.Api.Common.Database.Entities;

namespace Innovia.Api.Features.Resources.UpdateResource;

public sealed record Response(
    Guid Id,
    string Name,
    string Description,
    Guid ResourceTypeId,
    ResourceStatus Status
);
