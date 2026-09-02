using Innovia.Api.Common.Errors;

namespace Innovia.Api.Features.ResourceTypes;

public static class ResourceTypeErrors
{
    public static Error DuplicateName(string name) => Error.Conflict(
        $"Resource type with name '{name}' already exists."
    );
}
