using Innovia.Api.Common.Errors;
using Innovia.Api.Common.Result;

namespace Innovia.Api.Features.Auth.Register;

public sealed class Validator
{
    public ValidationResult Validate(Command cmd)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(cmd.FirstName))
            errors.Add(new ValidationError(nameof(cmd.FirstName), "First name is required"));

        if (string.IsNullOrWhiteSpace(cmd.LastName))
            errors.Add(new ValidationError(nameof(cmd.LastName), "Last name is required"));

        if (string.IsNullOrWhiteSpace(cmd.Email))
            errors.Add(new ValidationError(nameof(cmd.Email), "Email is required"));
        else if (!cmd.Email.Contains('@'))
            errors.Add(new ValidationError(nameof(cmd.Email), "Email is invalid"));

        if (string.IsNullOrWhiteSpace(cmd.Password))
            errors.Add(new ValidationError(nameof(cmd.Password), "Password is required"));
        else if (cmd.Password.Length < 8)
            errors.Add(new ValidationError(nameof(cmd.Password), "Password must be at least 8 characters"));

        return errors.Count == 0 ? ValidationResult.Success() : ValidationResult.Fail(errors);
    }
}
