namespace Innovia.Api.Common.Database.Entities;

public sealed class Resource
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public Guid ResourceTypeId { get; set; }
}
