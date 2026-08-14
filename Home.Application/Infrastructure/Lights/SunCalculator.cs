namespace Home.Application.Infrastructure.Lights;

/// <summary>
/// The classic Almanac sunrise equation — accurate to a couple of minutes, which is plenty for
/// deciding when the lounge lights come on. Pure maths, no clock and no I/O.
/// </summary>
public static class SunCalculator
{

    #region Fields

    private const double Degrees = Math.PI / 180.0;

    /// <summary>
    /// Cosine of the official zenith (90.833°) — the sun's centre half a degree below the
    /// horizon, which is what wall calendars call sunrise and sunset.
    /// </summary>
    private const double OfficialZenithCosine = -0.01454;

    #endregion Fields

    #region Methods

    /// <summary>
    /// The local time of sunrise or sunset on a date, or null when the sun never crosses the
    /// horizon there that day (polar day or night).
    /// </summary>
    public static TimeSpan? GetSunEventLocalTime(
        DateOnly date,
        double latitude,
        double longitude,
        TimeSpan utcOffset,
        bool sunrise)
    {
        var _LongitudeHour = longitude / 15.0;
        var _ApproximateTime = date.DayOfYear + (((sunrise ? 6.0 : 18.0) - _LongitudeHour) / 24.0);

        var _MeanAnomaly = (0.9856 * _ApproximateTime) - 3.289;

        var _TrueLongitude = Normalise(
            _MeanAnomaly
                + (1.916 * Math.Sin(_MeanAnomaly * Degrees))
                + (0.020 * Math.Sin(2 * _MeanAnomaly * Degrees))
                + 282.634,
            360.0);

        var _RightAscension = Normalise(Math.Atan(0.91764 * Math.Tan(_TrueLongitude * Degrees)) / Degrees, 360.0);

        // Pull the ascension into the same quadrant as the true longitude, then into hours.
        _RightAscension += (Math.Floor(_TrueLongitude / 90.0) * 90.0) - (Math.Floor(_RightAscension / 90.0) * 90.0);
        _RightAscension /= 15.0;

        var _SinDeclination = 0.39782 * Math.Sin(_TrueLongitude * Degrees);
        var _CosDeclination = Math.Cos(Math.Asin(_SinDeclination));

        var _CosHourAngle = (OfficialZenithCosine - (_SinDeclination * Math.Sin(latitude * Degrees)))
            / (_CosDeclination * Math.Cos(latitude * Degrees));

        if (_CosHourAngle is > 1.0 or < -1.0)
            return null;

        var _HourAngle = sunrise
            ? (360.0 - (Math.Acos(_CosHourAngle) / Degrees)) / 15.0
            : Math.Acos(_CosHourAngle) / Degrees / 15.0;

        var _MeanTime = _HourAngle + _RightAscension - (0.06571 * _ApproximateTime) - 6.622;
        var _UniversalTime = Normalise(_MeanTime - _LongitudeHour, 24.0);

        return TimeSpan.FromHours(Normalise(_UniversalTime + utcOffset.TotalHours, 24.0));
    }

    private static double Normalise(double value, double range)
    {
        var _Result = value % range;

        return _Result < 0 ? _Result + range : _Result;
    }

    #endregion Methods

}
