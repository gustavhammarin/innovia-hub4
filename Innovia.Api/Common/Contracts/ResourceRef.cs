namespace Innovia.Api.Common.Contracts;
public sealed record ResourceRef(
    Guid ResourceId,
    string Name,
    string Description,
    int Capacity
);  
