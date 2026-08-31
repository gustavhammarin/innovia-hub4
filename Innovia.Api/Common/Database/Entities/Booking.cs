namespace Innovia.Api.Common.Database.Entities;

public sealed class Booking
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid ResourceId { get; set; }
    public DateTimeOffset StartsAt { get; set; }
    public DateTimeOffset EndsAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}