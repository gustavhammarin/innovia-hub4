using Microsoft.AspNetCore.Identity;

namespace Innovia.Api.Common.Database.Entities;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
}