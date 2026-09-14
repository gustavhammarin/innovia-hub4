using Innovia.Api.Common.Database.Entities;

namespace Innovia.Api.Features.Resources.ListResourcesAdmin;

public sealed record Response(
    Guid Id,
    string Name,
    string Description,
    Guid ResourceTypeId,
    DateTimeOffset CreatedAt,
    ResourceStatus Status
);
