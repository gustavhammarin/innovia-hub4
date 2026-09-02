using System.ComponentModel.DataAnnotations;

namespace Innovia.Api.Features.Resources.UpdateResource;

public sealed record Command (Guid Id, string Name, Guid ResourceTypeId);