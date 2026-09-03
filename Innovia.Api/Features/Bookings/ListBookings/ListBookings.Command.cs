namespace Innovia.Api.Features.Bookings.ListBookings;

public sealed record Command(Guid? UserId, Guid? ResourceId, DateOnly? From, DateOnly? To);
