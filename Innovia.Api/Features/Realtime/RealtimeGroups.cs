namespace Innovia.Api.Features.Realtime;

public static class RealtimeGroups
{
    public static string Resource(Guid resourceId) => $"resource-{resourceId}";
    public static string AllBookings () => $"all-bookings-admin";
}