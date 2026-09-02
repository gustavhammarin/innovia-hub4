using Innovia.Api.Common.Errors;

namespace Innovia.Api.Features.Resources;

public static class ResourceErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "Booking Not Found"
    );

    public static Error InvalidReference() => Error.NotFound(
        "Invalid reference"
    );
}