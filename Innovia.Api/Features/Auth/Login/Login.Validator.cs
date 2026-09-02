using Innovia.Api.Common.Errors;
using Innovia.Api.Common.Result;

namespace Innovia.Api.Features.Auth.Login;

public sealed class Validator
{
    public ValidationResult Validate(Command cmd)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(cmd.Email))
            errors.Add(new ValidationError(nameof(cmd.Email), "Email is required"));
        if (string.IsNullOrWhiteSpace(cmd.Password))
            errors.Add(new ValidationError(nameof(cmd.Password), "Password is required"));
        
        return errors.Count == 0 ? ValidationResult.Success() : ValidationResult.Fail(errors);
    } 
}