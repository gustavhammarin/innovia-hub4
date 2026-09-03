namespace Innovia.Api.Common.Database.Entities;

public sealed class AvailabilityRule
{
    public Guid Id { get; set; }
    public Guid ResourceTypeId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly OpensAt { get; set; }
    public TimeOnly ClosesAt { get; set; }
    public int SlotDurationMinutes { get; set; }
}
