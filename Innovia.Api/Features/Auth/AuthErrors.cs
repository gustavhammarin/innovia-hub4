using Innovia.Api.Common.Errors;

namespace Innovia.Api.Features.Auth;

public static class AuthErrors
{
    public static Error InvalidCredentials() =>
        new("Invalid credentials", ErrorType.Forbidden);

    public static Error EmailAlreadyExists() =>
        new("Email already exists", ErrorType.Conflict);

    public static Error RegistrationFailed(string message) =>
        new(message, ErrorType.Validation);
}