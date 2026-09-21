using Innovia.Api.Common.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Innovia.Api.Features.Realtime;

[Authorize]
public class BookingHub : Hub
{
    public Task JoinResourceGroup(Guid resourceId) =>
        Groups.AddToGroupAsync(Context.ConnectionId, RealtimeGroups.Resource(resourceId));

    public Task LeaveResourceGroup(Guid resourceId) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, RealtimeGroups.Resource(resourceId));

    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    public Task JoinAdminBookingsGroup() =>
        Groups.AddToGroupAsync(Context.ConnectionId, RealtimeGroups.AllBookings());

    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    public Task LeaveAdminBookingsGroup () =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, RealtimeGroups.AllBookings());
}