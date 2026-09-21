using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Innovia.Api.Features.Realtime;

[Authorize]
public class ResourceHub : Hub
{
    public Task JoinResourceStatusGroup () => 
        Groups.AddToGroupAsync(Context.ConnectionId, RealtimeGroups.AllResourceStatuses());

    public Task LeaveResourceStatusGroup () =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, RealtimeGroups.AllResourceStatuses());

} 