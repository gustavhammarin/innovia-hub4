namespace Innovia.Api.Features.Bookings;

public static class BookingServiceExtensions
{
    public static IServiceCollection AddBookingsFeature(this IServiceCollection services)
    {
        services.AddScoped<CreateBooking.Handler>();
        services.AddScoped<CreateBooking.Validator>();

        return services;
    }

    public static IEndpointRouteBuilder MapBookingsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/bookings").WithTags("Bookings");

        CreateBooking.Endpoint.Map(group);

        return group;
    }
}