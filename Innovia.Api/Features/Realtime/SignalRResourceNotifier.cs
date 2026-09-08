using Innovia.Api.Common.Database.Entities;
using Innovia.Api.Features.Resources;
using Microsoft.AspNetCore.SignalR;

namespace Innovia.Api.Features.Realtime;

public sealed class SignalRResourceNotifier : IResourceNotifier
{
    private readonly IHubContext<ResourceHub> _hub;

    public SignalRResourceNotifier (IHubContext<ResourceHub> hub) 
    {
        _hub = hub;
    }
    public async Task ResourceStatusChangedAsync(Guid resourceId, ResourceStatus newStatus, CancellationToken ct = default)
    {
        await _hub.Clients.Group(RealtimeGroups.AllResourceStatuses())
            .SendAsync(ResourceEvents.ResourceStatusChanged, new { resourceId, newStatus}, ct);
    }
}