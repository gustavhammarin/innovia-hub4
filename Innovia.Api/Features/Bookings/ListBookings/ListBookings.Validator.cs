using Innovia.Api.Common.Errors;
using Innovia.Api.Common.Result;

namespace Innovia.Api.Features.Bookings.ListBookings;

public sealed class Validator
{
    public ValidationResult Validate(Command command)
    {
        var errors = new List<ValidationError>();

        if (command.From is not null && command.To is not null && command.From > command.To)
            errors.Add(new ValidationError(nameof(command.From), "From must be before or equal to To"));

        return errors.Count == 0 ? ValidationResult.Success() : ValidationResult.Fail(errors);
    }
}
