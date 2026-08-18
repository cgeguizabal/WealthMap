using WealthMap.Application.Common.Interfaces;

namespace WealthMap.Api.Auth;

/// <summary>
/// Reads the caller's time zone from the <c>X-Time-Zone</c> request header.
/// </summary>
/// <remarks>
/// A header rather than a query parameter on each route: every screen needs it,
/// and adding a parameter to forty endpoints would guarantee some of them were
/// missed — silently, since the fallback still returns an answer.
///
/// Registered scoped, so the lookup happens once per request rather than once per
/// property read.
/// </remarks>
public sealed class HttpUserClock : IUserClock
{
    public const string HeaderName = "X-Time-Zone";

    private readonly Lazy<TimeZoneInfo> _zone;

    public HttpUserClock(IHttpContextAccessor accessor)
    {
        _zone = new Lazy<TimeZoneInfo>(() =>
            Resolve(accessor.HttpContext?.Request.Headers[HeaderName].ToString()));
    }

    public TimeZoneInfo Zone => _zone.Value;

    public DateTime UtcNow => DateTime.UtcNow;

    public DateOnly Today =>
        DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Zone));

    /// <summary>
    /// Never throws. A request is not worth failing because a browser reported a
    /// zone this machine has never heard of; UTC is the old behaviour and is
    /// wrong only at the edges. Windows accepts IANA ids through ICU, so
    /// "America/Guatemala" resolves on both Windows and Linux.
    /// </summary>
    public static TimeZoneInfo Resolve(string? id)
    {
        if (string.IsNullOrWhiteSpace(id)) return TimeZoneInfo.Utc;

        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(id);
        }
        catch (Exception e) when (e is TimeZoneNotFoundException or InvalidTimeZoneException)
        {
            return TimeZoneInfo.Utc;
        }
    }
}
