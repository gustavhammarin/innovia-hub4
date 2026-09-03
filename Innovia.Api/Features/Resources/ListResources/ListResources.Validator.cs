using Innovia.Api.Common.Errors;
using Innovia.Api.Common.Result;

namespace Innovia.Api.Features.Resources.ListResources;

public sealed class Validator
{
    public ValidationResult Validate (Query query)
    {
        return ValidationResult.Success();
    }
    
}