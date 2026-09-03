namespace Innovia.Api.Common.Time;

public static class SwedenTimeZone
{
    public static readonly TimeZoneInfo Instance =
        TimeZoneInfo.FindSystemTimeZoneById("Europe/Stockholm");

    public static DateTimeOffset ToUtc(DateOnly date, TimeOnly localTime)
    {
        var localDateTime = DateTime.SpecifyKind(
            date.ToDateTime(localTime),
            DateTimeKind.Unspecified);

        var utcDateTime = TimeZoneInfo.ConvertTimeToUtc(localDateTime, Instance);
        return new DateTimeOffset(utcDateTime, TimeSpan.Zero);
    }
}
