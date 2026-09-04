using Innovia.Api.Common.Errors;

namespace Innovia.Api.Features.Resources;

public static class ResourceErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "Resource Not Found"
    );

    public static Error InvalidReference() => Error.NotFound(
        "Invalid reference"
    );

    public static readonly Error HasActiveBookings = Error.Conflict(
        "Resource cannot be archived because it has active or upcoming bookings."
    );

    public static readonly Error ArchivedCannotBeUpdated = Error.Conflict(
        "Archived resource cannot be updated."
    );

    public static readonly Error ArchivedCannotChangeStatus = Error.Conflict(
        "Archived resource status cannot be changed. Unarchive the resource first."
    );
}
