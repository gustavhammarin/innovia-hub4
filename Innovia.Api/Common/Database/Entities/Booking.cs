namespace Innovia.Api.Common.Database.Entities;

public sealed class Booking
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid ResourceId { get; set; }
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }
    public BookingStatus Status { get; set; }
}

public enum BookingStatus
{
    Available,
    Booked
}