namespace Home.Domain.Enumerations;

/// <summary>
/// Cooking and shopping measurements. Australian metric cups and spoons — a tablespoon here is
/// 20 ml, not the 15 ml the rest of the world uses, so imported recipes are never converted;
/// the amount is kept as the source wrote it alongside the unit it was written in.
/// </summary>
public class MeasurementUnitSE : BaseEnumeration
{

    #region Fields

    public static MeasurementUnitSE Pieces = new("Pieces", 1, string.Empty);
    public static MeasurementUnitSE Grams = new("Grams", 2, "g");
    public static MeasurementUnitSE Kilograms = new("Kilograms", 3, "kg");
    public static MeasurementUnitSE Millilitres = new("Millilitres", 4, "ml");
    public static MeasurementUnitSE Litres = new("Litres", 5, "L");
    public static MeasurementUnitSE Teaspoons = new("Teaspoons", 6, "tsp");
    public static MeasurementUnitSE Tablespoons = new("Tablespoons", 7, "tbsp");
    public static MeasurementUnitSE Cups = new("Cups", 8, "cups");
    public static MeasurementUnitSE Pinch = new("Pinch", 9, "pinch");
    public static MeasurementUnitSE Bunch = new("Bunch", 10, "bunch");
    public static MeasurementUnitSE Slices = new("Slices", 11, "slices");
    public static MeasurementUnitSE Cloves = new("Cloves", 12, "cloves");
    public static MeasurementUnitSE Tins = new("Tins", 13, "tins");
    public static MeasurementUnitSE Packets = new("Packets", 14, "packets");

    #endregion Fields

    #region Constructors

    public MeasurementUnitSE(string name, long value, string abbreviation) : base(name, value)
        => this.Abbreviation = abbreviation;

    #endregion Constructors

    #region Properties

    /// <summary>
    /// How the unit reads beside an amount on a card or a shopping list.
    /// </summary>
    public string Abbreviation { get; } = string.Empty;

    #endregion Properties

    #region Methods

    public static implicit operator MeasurementUnitSE(string name)
        => FromName<MeasurementUnitSE>(name) ?? throw new ArgumentException($"'{name}' is not a recognised {nameof(MeasurementUnitSE)} name.", nameof(name));

    public static implicit operator MeasurementUnitSE(long value)
        => FromValue<MeasurementUnitSE>(value) ?? throw new ArgumentException($"'{value}' is not a recognised {nameof(MeasurementUnitSE)} value.", nameof(value));

    #endregion Methods

}
