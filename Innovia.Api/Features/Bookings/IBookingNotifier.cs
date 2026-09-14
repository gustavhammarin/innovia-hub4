namespace Innovia.Api.Features.Bookings;

public interface IBookingNotifier
{
    Task BookingCreatedAsync(Guid resourceId, CancellationToken ct = default);
    Task BookingCancelledAsync(Guid resourceId, CancellationToken ct = default);
    Task BookingUpdatedAsync(Guid oldResourceId, Guid newResourceId, CancellationToken ct = default);
}