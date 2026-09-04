using Innovia.Api.Common.Database;
using Innovia.Api.Common.Database.Entities;
using Innovia.Api.Features.Bookings;
using Innovia.Api.Features.Resources;
using DeleteResourceHandler = Innovia.Api.Features.Resources.DeleteResource.Handler;
using DeleteResourceCommand = Innovia.Api.Features.Resources.DeleteResource.Command;
using UnarchiveResourceHandler = Innovia.Api.Features.Resources.UnarchiveResource.Handler;
using UnarchiveResourceCommand = Innovia.Api.Features.Resources.UnarchiveResource.Command;
using SetResourceMaintenanceHandler = Innovia.Api.Features.Resources.SetResourceMaintenance.Handler;
using SetResourceMaintenanceCommand = Innovia.Api.Features.Resources.SetResourceMaintenance.Command;
using SetResourceOnlineHandler = Innovia.Api.Features.Resources.SetResourceOnline.Handler;
using SetResourceOnlineCommand = Innovia.Api.Features.Resources.SetResourceOnline.Command;
using SetResourceOfflineHandler = Innovia.Api.Features.Resources.SetResourceOffline.Handler;
using SetResourceOfflineCommand = Innovia.Api.Features.Resources.SetResourceOffline.Command;
using UpdateResourceHandler = Innovia.Api.Features.Resources.UpdateResource.Handler;
using UpdateResourceCommand = Innovia.Api.Features.Resources.UpdateResource.Command;
using ListResourcesHandler = Innovia.Api.Features.Resources.ListResources.Handler;
using ListResourcesAdminHandler = Innovia.Api.Features.Resources.ListResourcesAdmin.Handler;
using GetResourceByIdHandler = Innovia.Api.Features.Resources.GetResourceById.Handler;
using GetResourceByIdCommand = Innovia.Api.Features.Resources.GetResourceById.Command;
using GetResourceByIdAdminHandler = Innovia.Api.Features.Resources.GetResourceByIdAdmin.Handler;
using GetResourceByIdAdminCommand = Innovia.Api.Features.Resources.GetResourceByIdAdmin.Command;

namespace Innovia.Api.Tests.Features.ResourceTests;

[Collection("Database")]
public class ResourceStatusTests
{
    private readonly DatabaseFixture _fixture;

    public ResourceStatusTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    private static async Task<(Guid UserId, Guid ResourceId)> SeedResourceAsync(
        AppDbContext context,
        ResourceStatus status = ResourceStatus.Online)
    {
        var unique = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = Guid.CreateVersion7(),
            UserName = $"{unique}@test.com",
            Email = $"{unique}@test.com"
        };

        var resourceType = new ResourceType
        {
            Id = Guid.CreateVersion7(),
            Name = $"Room {unique}",
            CreatedAt = DateTimeOffset.UtcNow,
            MaxDurationMinutes = 480,
            MaxAdvanceDays = 90
        };

        var resource = new Resource
        {
            Id = Guid.CreateVersion7(),
            Name = $"Meeting Room {unique}",
            CreatedAt = DateTimeOffset.UtcNow,
            ResourceTypeId = resourceType.Id,
            Description = string.Empty,
            Status = status
        };

        var availabilityRules = Enum.GetValues<DayOfWeek>()
            .Select(dayOfWeek => new AvailabilityRule
            {
                Id = Guid.CreateVersion7(),
                ResourceTypeId = resourceType.Id,
                DayOfWeek = dayOfWeek,
                OpensAt = new TimeOnly(0, 0),
                ClosesAt = new TimeOnly(23, 59),
                SlotDurationMinutes = 60
            });

        context.Users.Add(user);
        context.ResourceTypes.Add(resourceType);
        context.Resources.Add(resource);
        context.AvailabilityRules.AddRange(availabilityRules);
        await context.SaveChangesAsync();

        return (user.Id, resource.Id);
    }

    private static async Task<Guid> SeedResourceTypeAsync(AppDbContext context)
    {
        var resourceType = new ResourceType
        {
            Id = Guid.CreateVersion7(),
            Name = $"Room {Guid.NewGuid()}",
            CreatedAt = DateTimeOffset.UtcNow,
            MaxDurationMinutes = 480,
            MaxAdvanceDays = 90
        };

        context.ResourceTypes.Add(resourceType);
        await context.SaveChangesAsync();

        return resourceType.Id;
    }

    [Fact]
    public async Task DeleteResource_Should_Archive_Resource()
    {
        await using var context = _fixture.CreateDbContext();
        var (_, resourceId) = await SeedResourceAsync(context);
        var handler = new DeleteResourceHandler(context);

        var result = await handler.HandleAsync(new DeleteResourceCommand(resourceId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(ResourceStatus.Archived, result.Value!.Status);
        Assert.Equal(ResourceStatus.Archived, context.Resources.Single(r => r.Id == resourceId).Status);
    }

    [Fact]
    public async Task DeleteResource_Should_Fail_When_Resource_Has_Booking()
    {
        await using var context = _fixture.CreateDbContext();
        var (userId, resourceId) = await SeedResourceAsync(context);
        context.Bookings.Add(new Booking
        {
            Id = Guid.CreateVersion7(),
            ResourceId = resourceId,
            UserId = userId,
            StartsAt = DateTimeOffset.UtcNow.AddHours(1),
            EndsAt = DateTimeOffset.UtcNow.AddHours(2),
            CreatedAt = DateTimeOffset.UtcNow
        });
        await context.SaveChangesAsync();

        var handler = new DeleteResourceHandler(context);
        var result = await handler.HandleAsync(new DeleteResourceCommand(resourceId), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResourceErrors.HasActiveBookings, result.Error);
        Assert.Equal(ResourceStatus.Online, context.Resources.Single(r => r.Id == resourceId).Status);
    }

    [Fact]
    public async Task UnarchiveResource_Should_Set_Status_To_Online()
    {
        await using var context = _fixture.CreateDbContext();
        var (_, resourceId) = await SeedResourceAsync(context, ResourceStatus.Archived);
        var handler = new UnarchiveResourceHandler(context);

        var result = await handler.HandleAsync(new UnarchiveResourceCommand(resourceId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(ResourceStatus.Online, result.Value!.Status);
        Assert.Equal(ResourceStatus.Online, context.Resources.Single(r => r.Id == resourceId).Status);
    }

    [Fact]
    public async Task BookingRules_Should_Fail_When_Resource_Is_Archived()
    {
        await using var context = _fixture.CreateDbContext();
        var (_, resourceId) = await SeedResourceAsync(context, ResourceStatus.Archived);
        var service = new BookingRulesService(context);

        var startsAt = DateTimeOffset.UtcNow.AddHours(1);
        var result = await service.ValidateAsync(resourceId, startsAt, startsAt.AddHours(1), CancellationToken.None);

        Assert.Equal(BookingErrors.ResourceUnavailable(ResourceStatus.Archived), result);
    }

    [Fact]
    public async Task SetResourceMaintenance_Should_Set_Status_To_Maintenance()
    {
        await using var context = _fixture.CreateDbContext();
        var (_, resourceId) = await SeedResourceAsync(context);
        var handler = new SetResourceMaintenanceHandler(context);

        var result = await handler.HandleAsync(new SetResourceMaintenanceCommand(resourceId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(ResourceStatus.Maintenance, result.Value!.Status);
        Assert.Equal(ResourceStatus.Maintenance, context.Resources.Single(r => r.Id == resourceId).Status);
    }

    [Fact]
    public async Task SetResourceOnline_Should_Set_Status_To_Online()
    {
        await using var context = _fixture.CreateDbContext();
        var (_, resourceId) = await SeedResourceAsync(context, ResourceStatus.Maintenance);
        var handler = new SetResourceOnlineHandler(context);

        var result = await handler.HandleAsync(new SetResourceOnlineCommand(resourceId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(ResourceStatus.Online, result.Value!.Status);
        Assert.Equal(ResourceStatus.Online, context.Resources.Single(r => r.Id == resourceId).Status);
    }

    [Fact]
    public async Task SetResourceOffline_Should_Set_Status_To_Offline()
    {
        await using var context = _fixture.CreateDbContext();
        var (_, resourceId) = await SeedResourceAsync(context);
        var handler = new SetResourceOfflineHandler(context);

        var result = await handler.HandleAsync(new SetResourceOfflineCommand(resourceId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(ResourceStatus.Offline, result.Value!.Status);
        Assert.Equal(ResourceStatus.Offline, context.Resources.Single(r => r.Id == resourceId).Status);
    }

    [Fact]
    public async Task SetResourceMaintenance_Should_Fail_When_Resource_Is_Archived()
    {
        await using var context = _fixture.CreateDbContext();
        var (_, resourceId) = await SeedResourceAsync(context, ResourceStatus.Archived);
        var handler = new SetResourceMaintenanceHandler(context);

        var result = await handler.HandleAsync(new SetResourceMaintenanceCommand(resourceId), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResourceErrors.ArchivedCannotChangeStatus, result.Error);
        Assert.Equal(ResourceStatus.Archived, context.Resources.Single(r => r.Id == resourceId).Status);
    }

    [Fact]
    public async Task SetResourceOnline_Should_Fail_When_Resource_Is_Archived()
    {
        await using var context = _fixture.CreateDbContext();
        var (_, resourceId) = await SeedResourceAsync(context, ResourceStatus.Archived);
        var handler = new SetResourceOnlineHandler(context);

        var result = await handler.HandleAsync(new SetResourceOnlineCommand(resourceId), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResourceErrors.ArchivedCannotChangeStatus, result.Error);
        Assert.Equal(ResourceStatus.Archived, context.Resources.Single(r => r.Id == resourceId).Status);
    }

    [Fact]
    public async Task SetResourceOffline_Should_Fail_When_Resource_Is_Archived()
    {
        await using var context = _fixture.CreateDbContext();
        var (_, resourceId) = await SeedResourceAsync(context, ResourceStatus.Archived);
        var handler = new SetResourceOfflineHandler(context);

        var result = await handler.HandleAsync(new SetResourceOfflineCommand(resourceId), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResourceErrors.ArchivedCannotChangeStatus, result.Error);
        Assert.Equal(ResourceStatus.Archived, context.Resources.Single(r => r.Id == resourceId).Status);
    }

    [Fact]
    public async Task BookingRules_Should_Fail_When_Resource_Is_On_Maintenance()
    {
        await using var context = _fixture.CreateDbContext();
        var (_, resourceId) = await SeedResourceAsync(context, ResourceStatus.Maintenance);
        var service = new BookingRulesService(context);

        var startsAt = DateTimeOffset.UtcNow.AddHours(1);
        var result = await service.ValidateAsync(resourceId, startsAt, startsAt.AddHours(1), CancellationToken.None);

        Assert.Equal(BookingErrors.ResourceUnavailable(ResourceStatus.Maintenance), result);
    }

    [Fact]
    public async Task UpdateResource_Should_Update_Description()
    {
        await using var context = _fixture.CreateDbContext();
        var (_, resourceId) = await SeedResourceAsync(context);
        var resourceTypeId = await SeedResourceTypeAsync(context);
        var handler = new UpdateResourceHandler(context);

        var result = await handler.HandleAsync(
            new UpdateResourceCommand(resourceId, "Updated resource", "Updated description", resourceTypeId),
            CancellationToken.None);

        var resource = context.Resources.Single(r => r.Id == resourceId);
        Assert.True(result.IsSuccess);
        Assert.Equal("Updated description", result.Value!.Description);
        Assert.Equal("Updated description", resource.Description);
    }

    [Fact]
    public async Task ListResources_Should_Exclude_Archived_Resources()
    {
        await using var context = _fixture.CreateDbContext();
        var (_, onlineResourceId) = await SeedResourceAsync(context);
        var (_, archivedResourceId) = await SeedResourceAsync(context, ResourceStatus.Archived);
        var handler = new ListResourcesHandler(context);

        var result = await handler.HandleAsync(CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Contains(result.Value!, resource => resource.Id == onlineResourceId);
        Assert.DoesNotContain(result.Value!, resource => resource.Id == archivedResourceId);
    }

    [Fact]
    public async Task ListResourcesAdmin_Should_Include_Archived_Resources()
    {
        await using var context = _fixture.CreateDbContext();
        var (_, onlineResourceId) = await SeedResourceAsync(context);
        var (_, archivedResourceId) = await SeedResourceAsync(context, ResourceStatus.Archived);
        var handler = new ListResourcesAdminHandler(context);

        var result = await handler.HandleAsync(CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Contains(result.Value!, resource => resource.Id == onlineResourceId);
        Assert.Contains(result.Value!, resource => resource.Id == archivedResourceId);
    }

    [Fact]
    public async Task GetResourceById_Should_Not_Return_Archived_Resource()
    {
        await using var context = _fixture.CreateDbContext();
        var (_, resourceId) = await SeedResourceAsync(context, ResourceStatus.Archived);
        var handler = new GetResourceByIdHandler(context);

        var result = await handler.HandleAsync(new GetResourceByIdCommand(resourceId), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResourceErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task GetResourceByIdAdmin_Should_Return_Archived_Resource()
    {
        await using var context = _fixture.CreateDbContext();
        var (_, resourceId) = await SeedResourceAsync(context, ResourceStatus.Archived);
        var handler = new GetResourceByIdAdminHandler(context);

        var result = await handler.HandleAsync(new GetResourceByIdAdminCommand(resourceId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(resourceId, result.Value!.Id);
        Assert.Equal(ResourceStatus.Archived, result.Value.Status);
    }
}
