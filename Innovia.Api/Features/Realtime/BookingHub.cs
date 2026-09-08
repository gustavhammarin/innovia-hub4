using Microsoft.AspNetCore.SignalR;

namespace Innovia.Api.Features.Realtime;

public class BookingHub : Hub
{
    public Task JoinResourceGroup(Guid resourceId) =>
        Groups.AddToGroupAsync(Context.ConnectionId, RealtimeGroups.Resource(resourceId));
    
    public Task LeaveResourceGroup(Guid resourceId) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, RealtimeGroups.Resource(resourceId));
}