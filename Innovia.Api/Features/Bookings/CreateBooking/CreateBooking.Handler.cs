using Innovia.Api.Common.Database;
using Innovia.Api.Common.Database.Entities;
using Innovia.Api.Common.Result;

namespace Innovia.Api.Features.Bookings.CreateBooking;

public sealed class Handler
{
    private readonly AppDbContext _context;

    public async Task<Result<Response>> HandleAsync(Command cmd, CancellationToken ct)
    {
        var booking = new Booking
        {
            Id = Guid.CreateVersion7(),
            ResourceId = cmd.ResourceId,
            UserId = cmd.UserId,
            StartsAt = cmd.StartsAt,
            EndsAt = cmd.EndsAt,
            CreatedAt = DateTimeOffset.UtcNow
        };

        throw new NotImplementedException();
    }
}