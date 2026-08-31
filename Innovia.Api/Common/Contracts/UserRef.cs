namespace Innovia.Api.Common.Contracts;
public sealed record UserRef(
    Guid UserId,
    string Email
);