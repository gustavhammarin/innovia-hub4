using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Innovia.Api.Common.Auth;
using Innovia.Api.Common.Result;

namespace Innovia.Api.Features.Bookings.ListBookingsByUserId
{
    public static class Endpoint
    {
        public static RouteHandlerBuilder Map(IEndpointRouteBuilder app)
        {
            return app.MapGet("/user/{userId:guid}", async (
                Guid userId,
                Handler handler,
                Validator validator,
                ICurrentUser currentuser,
                CancellationToken ct
                ) =>
            {
                var command = new Command(userId, currentuser.UserId!.Value, currentuser.IsAdmin);

                var validation = validator.Validate(command);
                if(!validation.IsValid)
                    return validation.ToProblemResult();

                    var result = await handler.HandleAsync(command,ct);
                    return result.ToHttpResponse();
            });
        }
    }
}