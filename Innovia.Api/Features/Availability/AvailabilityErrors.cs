using Innovia.Api.Common.Errors;

namespace Innovia.Api.Features.Availability;

public static class AvailabilityErrors
{
    public static readonly Error RuleNotFound = Error.NotFound(
        "Availability rule was not found."
    );

    public static Error ResourceNotFound(Guid resourceId) => Error.NotFound(
        $"Resource '{resourceId}' was not found."
    );

    public static Error ResourceTypeNotFound(Guid resourceTypeId) => Error.NotFound(
        $"Resource type '{resourceTypeId}' was not found."
    );

    public static Error DuplicateRule(Guid resourceTypeId, DayOfWeek dayOfWeek) => Error.Conflict(
        $"Availability rule for resource type '{resourceTypeId}' on '{dayOfWeek}' already exists."
    );
}
