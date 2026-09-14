using Innovia.Api.Common.Database.Entities;

namespace Innovia.Api.Features.Realtime;

public interface IResourceNotifier
{
    Task ResourceStatusChangedAsync (Guid resourceId, ResourceStatus newStatus, CancellationToken ct = default);
}