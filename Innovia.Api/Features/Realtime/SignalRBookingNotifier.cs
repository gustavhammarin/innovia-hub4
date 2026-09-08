using Innovia.Api.Features.Bookings;
using Microsoft.AspNetCore.SignalR;

namespace Innovia.Api.Features.Realtime;

public sealed class SignalRBookingNotifier : IBookingNotifier
{
    private readonly IHubContext<BookingHub> _hub;
    public SignalRBookingNotifier(IHubContext<BookingHub> hub)
    {
        _hub = hub;
    }
    public async Task BookingCancelledAsync(Guid resourceId, CancellationToken ct = default)
    {
        await _hub.Clients.Group(RealtimeGroups.Resource(resourceId))
            .SendAsync(BookingEvents.BookingCancelled, new {resourceId}, ct);
    }

    public async Task BookingCreatedAsync(Guid resourceId, CancellationToken ct = default)
    {
        await _hub.Clients.Group(RealtimeGroups.Resource(resourceId))
            .SendAsync(BookingEvents.BookingCreated, new {resourceId}, ct);

        await _hub.Clients.Group(RealtimeGroups.AllBookings())
            .SendAsync(BookingEvents.BookingCreated, new {resourceId}, ct);
    }

    public async Task BookingUpdatedAsync(Guid oldResourceId, Guid newResourceId, CancellationToken ct = default)
    {
        await _hub.Clients.Group(RealtimeGroups.Resource(oldResourceId))
            .SendAsync(BookingEvents.BookingUpdated, new {oldResourceId}, ct);
        
        if (newResourceId != oldResourceId)
        {
            await _hub.Clients.Group(RealtimeGroups.Resource(newResourceId))
            .SendAsync(BookingEvents.BookingUpdated, new {newResourceId}, ct);
        }
    }
}