
namespace Innovia.Api.Features.Bookings.ListBookingsByUserId;

public sealed record Command(Guid RequestedUserId, Guid CurrentUserId, bool IsAdmin);