using Innovia.Api.Common.Database.Entities;

namespace Innovia.Api.Features.Resources.SetResourceMaintenance;

public sealed record Response(
    Guid Id,
    ResourceStatus Status
);
