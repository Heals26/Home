using Home.Domain.Enumerations;

namespace Home.Application.Infrastructure.Recipes;

/// <summary>
/// The one place that turns a stored unit value into something readable, and the one place that
/// decides whether a value the caller sent is a unit at all.
/// </summary>
public static class MeasurementUnitLogic
{

    #region Methods

    /// <summary>
    /// The form that suits the amount beside it: exactly one reads singular, anything else —
    /// including a half and an amount nobody has given yet — reads plural.
    /// </summary>
    public static string GetAbbreviation(long? unit, decimal? amount)
    {
        if (unit == null)
            return string.Empty;

        var _Unit = BaseEnumeration.FromValue<MeasurementUnitSE>(unit.Value);

        if (_Unit == null)
            return string.Empty;

        return amount == 1
            ? _Unit.SingularAbbreviation
            : _Unit.Abbreviation;
    }

    /// <summary>
    /// Null is defined — an amount with no unit is a count of somethings.
    /// </summary>
    public static bool IsDefined(long? unit)
        => unit == null || BaseEnumeration.FromValue<MeasurementUnitSE>(unit.Value) != null;

    #endregion Methods

}
