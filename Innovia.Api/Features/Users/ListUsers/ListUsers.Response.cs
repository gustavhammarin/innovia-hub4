namespace Innovia.Api.Features.Users.ListUsers;

public sealed record UserResponse(
    Guid Id,
    string FullName,
    string Email
);
