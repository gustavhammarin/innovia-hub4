using Innovia.Api.Common.Result;
using Innovia.Api.Common.Errors;
namespace Innovia.Api.Features.Users;
public static class UserErrors
{
    public static Error NotFound => Error.NotFound("User not found.");
    public static Error Invalid(string message) => Error.Validation(message);
}
