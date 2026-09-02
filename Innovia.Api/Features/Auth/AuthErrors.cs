using Innovia.Api.Common.Errors;

namespace Innovia.Api.Features.Auth;

public static class AuthErrors
{
    public static Error InvalidCredentials() =>
        new("Invalid credentials", ErrorType.Forbidden);
}