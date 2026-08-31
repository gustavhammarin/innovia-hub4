namespace Innovia.Api.Common.Database.Entities;

public sealed class ResourceType
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}