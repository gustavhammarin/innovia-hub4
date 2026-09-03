using Innovia.Api.Common.Contracts;
using Innovia.Api.Common.Database;
using Innovia.Api.Common.Result;
using Innovia.Api.Common.Time;
using Microsoft.EntityFrameworkCore;

namespace Innovia.Api.Features.Bookings.ListBookings;

public sealed class Handler
{
    private readonly AppDbContext _context;

    public Handler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<BookingResponse>>> HandleAsync(Command command, CancellationToken ct)
    {
        var query = _context.Bookings.AsNoTracking().AsQueryable();

        if (command.UserId is not null)
            query = query.Where(b => b.UserId == command.UserId);

        if (command.ResourceId is not null)
            query = query.Where(b => b.ResourceId == command.ResourceId);

        if (command.From is not null)
        {
            var fromUtc = SwedenTimeZone.ToUtc(command.From.Value, TimeOnly.MinValue);
            query = query.Where(b => b.StartsAt >= fromUtc);
        }

        if (command.To is not null)
        {
            var toUtc = SwedenTimeZone.ToUtc(command.To.Value.AddDays(1), TimeOnly.MinValue);
            query = query.Where(b => b.StartsAt < toUtc);
        }

        var bookings = await query.ToListAsync(ct);

        if (bookings.Count == 0)
            return Result<List<BookingResponse>>.Ok([]);

        var resourceIds = bookings.Select(b => b.ResourceId).Distinct().ToList();
        var userIds = bookings.Select(b => b.UserId).Distinct().ToList();

        var resources = await _context.Resources
            .AsNoTracking()
            .Where(r => resourceIds.Contains(r.Id))
            .Select(r => new { r.Id, r.Name, r.Description })
            .ToDictionaryAsync(r => r.Id, ct);

        var users = await _context.Users
            .AsNoTracking()
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new { u.Id, u.Email })
            .ToDictionaryAsync(u => u.Id, ct);

        var responses = bookings.Select(b => new BookingResponse(
            b.Id,
            new UserRef(users[b.UserId].Id, users[b.UserId].Email ?? "Unknown"),
            new ResourceRef(resources[b.ResourceId].Id, resources[b.ResourceId].Name, resources[b.ResourceId].Description),
            b.StartsAt,
            b.EndsAt,
            b.CreatedAt
        )).ToList();

        return Result<List<BookingResponse>>.Ok(responses);
    }
}
