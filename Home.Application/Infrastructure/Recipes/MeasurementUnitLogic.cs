using Home.Domain.Enumerations;

namespace Home.Application.Infrastructure.Recipes;

/// <summary>
/// The one place that turns a stored unit value into something readable, and the one place that
/// decides whether a value the caller sent is a unit at all.
/// </summary>
public static class MeasurementUnitLogic
{

    #region Methods

    public static string GetAbbreviation(long? unit)
        => unit == null
            ? string.Empty
            : BaseEnumeration.FromValue<MeasurementUnitSE>(unit.Value)?.Abbreviation ?? string.Empty;

    /// <summary>
    /// Null is defined — an amount with no unit is a count of somethings.
    /// </summary>
    public static bool IsDefined(long? unit)
        => unit == null || BaseEnumeration.FromValue<MeasurementUnitSE>(unit.Value) != null;

    #endregion Methods

}
