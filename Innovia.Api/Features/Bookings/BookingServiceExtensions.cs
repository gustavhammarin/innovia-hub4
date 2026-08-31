namespace Innovia.Api.Features.Bookings;

public static class BookingServiceExtensions
{
    public static IServiceCollection AddBookingsFeature(this IServiceCollection services)
    {
        services.AddScoped<CreateBooking.Handler>();
        services.AddScoped<CreateBooking.Validator>();

        return services;
    }
}