using Innovia.Api.Common.Auth;

namespace Innovia.Api.Features.Bookings;

public static class BookingServiceExtensions
{
    public static IServiceCollection AddBookingsFeature(this IServiceCollection services)
    {
        services.AddScoped<CreateBooking.Handler>();
        services.AddScoped<CreateBooking.Validator>();
        services.AddScoped<UpdateBooking.Handler>();
        services.AddScoped<UpdateBooking.Validator>();

        services.AddScoped<GetBookingById.Handler>();
        services.AddScoped<GetBookingById.Validator>();

        services.AddScoped<ListAllBookings.Handler>();

        services.AddScoped<CancelBooking.Handler>();
        services.AddScoped<CancelBooking.Validator>();

        return services;
    }

    public static IEndpointRouteBuilder MapBookingsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/bookings").WithTags("Bookings");

        CreateBooking.Endpoint.Map(group)
            .RequireAuthorization(AuthorizationPolicies.MemberOrAdmin);

        UpdateBooking.Endpoint.Map(group)
            .RequireAuthorization(AuthorizationPolicies.MemberOrAdmin);

        GetBookingById.Endpoint.Map(group)
            .RequireAuthorization(AuthorizationPolicies.MemberOrAdmin);

        ListAllBookings.Endpoint.Map(group)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly);

        CancelBooking.Endpoint.Map(group)
            .RequireAuthorization(AuthorizationPolicies.MemberOrAdmin);

        return group;
    }
}
