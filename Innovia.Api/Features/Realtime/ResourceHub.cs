using Microsoft.AspNetCore.SignalR;

namespace Innovia.Api.Features.Realtime;

public class ResourceHub : Hub
{
    public Task JoinResourceStatusGroup () => 
        Groups.AddToGroupAsync(Context.ConnectionId, RealtimeGroups.AllResourceStatuses());

    public Task LeaveResourceStatusGrou () => 
        Groups.RemoveFromGroupAsync(Context.ConnectionId, RealtimeGroups.AllResourceStatuses());

} 