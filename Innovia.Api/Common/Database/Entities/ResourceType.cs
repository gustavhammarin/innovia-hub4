namespace Innovia.Api.Common.Database.Entities;

public sealed class ResourceType
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public int MaxAdvanceDays { get; set; } = 90;
    public int MaxDurationMinutes {get; set;} = 480;
}