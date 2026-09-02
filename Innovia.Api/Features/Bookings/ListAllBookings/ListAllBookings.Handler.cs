using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Innovia.Api.Common.Contracts;
using Innovia.Api.Common.Database;
using Innovia.Api.Common.Result;
using Microsoft.EntityFrameworkCore;

namespace Innovia.Api.Features.Bookings.ListAllBookings
{
    public sealed class Handler
    {
        private readonly AppDbContext _context;

        public Handler (AppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<List<BookingResponse>>> HandleAsync (Command command, CancellationToken ct)
        {
            var bookings = await _context.Bookings
            .AsNoTracking()
            .ToListAsync(ct);

            var resourceIds = bookings.Select(b => b.ResourceId).Distinct().ToList();
            var userIds = bookings.Select(b => b.UserId).Distinct().ToList();

            var resources = await _context.Resources
            .AsNoTracking()
            .Where(r => resourceIds.Contains(r.Id))
            .Select(r => new {r.Id, r.Name, r.Description})
            .ToDictionaryAsync(r => r.Id, ct);

            var users = await _context.Users
            .AsNoTracking()
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new { u.Id, u.Email})
            .ToDictionaryAsync(u => u.Id, ct);

            var responses = bookings.Select(b => new BookingResponse(
                b.Id,
                new UserRef (users[b.UserId].Id, users[b.UserId].Email ?? "Unknown"),
                new ResourceRef (resources[b.ResourceId].Id, resources[b.ResourceId].Name, resources[b.ResourceId].Description),
                b.StartsAt,
                b.EndsAt,
                b.CreatedAt
            )).ToList();

            return Result<List<BookingResponse>>.Ok(responses);
        }
        
    }
}