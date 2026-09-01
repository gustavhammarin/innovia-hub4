using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Innovia.Api.Common.Errors;
using Innovia.Api.Common.Result;

namespace Innovia.Api.Features.Bookings.ListBookingsByUserId
{
    public sealed class Validator
    {
        public ValidationResult Validate (Command command)
        {
            var errors = new List<ValidationError>();

            if(command.RequestedUserId == Guid.Empty)
                errors.Add(new ValidationError(nameof(command.RequestedUserId), "UserId is required"));

                return errors.Count == 0
                ? ValidationResult.Success()
                : ValidationResult.Fail(errors);
        }
    }
}