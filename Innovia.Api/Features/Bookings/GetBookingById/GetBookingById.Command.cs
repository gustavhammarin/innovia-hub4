namespace Innovia.Api.Features.Bookings.GetBookingById;

public sealed record Command (Guid BookingId, Guid UserId, bool IsAdmin);