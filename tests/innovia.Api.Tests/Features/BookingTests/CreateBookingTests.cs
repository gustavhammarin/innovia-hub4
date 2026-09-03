using Innovia.Api.Common.Database;
using Innovia.Api.Common.Database.Entities;
using Innovia.Api.Features.Bookings;
using Innovia.Api.Features.Bookings.CreateBooking;

namespace Innovia.Api.Tests.Features.BookingTests;

[Collection("Database")]
public class CreateBookingTests
{
    private readonly DatabaseFixture _fixture;

    public CreateBookingTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    private static async Task<(Guid UserId, Guid ResourceId)> SeedUserAndResourceAsync(AppDbContext context)
    {
        var user = new ApplicationUser
        {
            Id = Guid.CreateVersion7(),
            UserName = $"{Guid.NewGuid()}@test.com",
            Email = $"{Guid.NewGuid()}@test.com"
        };

        var resourceType = new ResourceType
        {
            Id = Guid.CreateVersion7(),
            Name = "Room",
            CreatedAt = DateTimeOffset.UtcNow
        };

        var resource = new Resource
        {
            Id = Guid.CreateVersion7(),
            Name = "Meeting Room 1",
            CreatedAt = DateTimeOffset.UtcNow,
            ResourceTypeId = resourceType.Id,
            Description = string.Empty
        };

        context.Users.Add(user);
        context.ResourceTypes.Add(resourceType);
        context.Resources.Add(resource);
        await context.SaveChangesAsync();

        return (user.Id, resource.Id);
    }

    [Fact]
    public async Task Should_Succeed_When_Booking_Valid_Time_On_Resource()
    {
        await using var context = _fixture.CreateDbContext();
        var (userId, resourceId) = await SeedUserAndResourceAsync(context);
        var handler = new Handler(context);

        var startsAt = DateTimeOffset.UtcNow.AddHours(1);
        var command = new Command(resourceId, userId, startsAt, startsAt.AddHours(1));

        var result = await handler.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(resourceId, result.Value!.Resource.ResourceId);
        Assert.Equal(userId, result.Value.User.UserId);
        Assert.Equal(startsAt, result.Value.StartsAt);
    }

    [Fact]
    public async Task Should_Fail_With_Overlaps_When_Booking_Overlapping_Time_On_Same_Resource()
    {
        await using var context = _fixture.CreateDbContext();
        var (userId, resourceId) = await SeedUserAndResourceAsync(context);
        var handler = new Handler(context);

        var startsAt = DateTimeOffset.UtcNow.AddHours(2);
        var existing = new Command(resourceId, userId, startsAt, startsAt.AddHours(1));
        var existingResult = await handler.HandleAsync(existing, CancellationToken.None);
        Assert.True(existingResult.IsSuccess);

        var overlapping = new Command(resourceId, userId, startsAt.AddMinutes(30), startsAt.AddMinutes(90));
        var result = await handler.HandleAsync(overlapping, CancellationToken.None);

        Assert.True(!result.IsSuccess);
        Assert.Equal(BookingErrors.Overlaps, result.Error);
    }

    [Fact]
    public async Task Should_Succeed_When_Booking_Back_To_Back_On_Same_Resource()
    {
        await using var context = _fixture.CreateDbContext();
        var (userId, resourceId) = await SeedUserAndResourceAsync(context);
        var handler = new Handler(context);

        var startsAt = DateTimeOffset.UtcNow.AddHours(3);
        var first = new Command(resourceId, userId, startsAt, startsAt.AddHours(1));
        var firstResult = await handler.HandleAsync(first, CancellationToken.None);
        Assert.True(firstResult.IsSuccess);

        var backToBack = new Command(resourceId, userId, startsAt.AddHours(1), startsAt.AddHours(2));
        var result = await handler.HandleAsync(backToBack, CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Should_Succeed_When_Booking_Same_Time_On_Different_Resource()
    {
        await using var context = _fixture.CreateDbContext();
        var (userId, resourceId1) = await SeedUserAndResourceAsync(context);
        var (_, resourceId2) = await SeedUserAndResourceAsync(context);
        var handler = new Handler(context);

        var startsAt = DateTimeOffset.UtcNow.AddHours(4);
        var first = new Command(resourceId1, userId, startsAt, startsAt.AddHours(1));
        var firstResult = await handler.HandleAsync(first, CancellationToken.None);
        Assert.True(firstResult.IsSuccess);

        var second = new Command(resourceId2, userId, startsAt, startsAt.AddHours(1));
        var result = await handler.HandleAsync(second, CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Should_Fail_With_InvalidReference_When_ResourceId_Does_Not_Exist()
    {
        await using var context = _fixture.CreateDbContext();
        var user = new ApplicationUser
        {
            Id = Guid.CreateVersion7(),
            UserName = $"{Guid.NewGuid()}@test.com",
            Email = $"{Guid.NewGuid()}@test.com"
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var handler = new Handler(context);
        var startsAt = DateTimeOffset.UtcNow.AddHours(5);
        var command = new Command(Guid.CreateVersion7(), user.Id, startsAt, startsAt.AddHours(1));

        var result = await handler.HandleAsync(command, CancellationToken.None);

        Assert.True(!result.IsSuccess);
        Assert.Equal(BookingErrors.InvalidReference(), result.Error);
    }
}
