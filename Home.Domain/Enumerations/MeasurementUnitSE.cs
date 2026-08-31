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
    public static MeasurementUnitSE Cups = new("Cups", 8, "cups", "cup");
    public static MeasurementUnitSE Pinch = new("Pinch", 9, "pinches", "pinch");
    public static MeasurementUnitSE Bunch = new("Bunch", 10, "bunches", "bunch");
    public static MeasurementUnitSE Slices = new("Slices", 11, "slices", "slice");
    public static MeasurementUnitSE Cloves = new("Cloves", 12, "cloves", "clove");
    public static MeasurementUnitSE Tins = new("Tins", 13, "tins", "tin");
    public static MeasurementUnitSE Packets = new("Packets", 14, "packets", "packet");
    public static MeasurementUnitSE Jars = new("Jars", 15, "jars", "jar");
    public static MeasurementUnitSE Leaves = new("Leaves", 16, "leaves", "leaf");
    public static MeasurementUnitSE Stalks = new("Stalks", 17, "stalks", "stalk");
    public static MeasurementUnitSE Dashes = new("Dashes", 18, "dashes", "dash");

    #endregion Fields

    #region Constructors

    /// <summary>
    /// A unit written as a symbol reads the same however many there are, so it passes one form.
    /// A unit written as a word passes both, because English does not derive them reliably —
    /// a leaf becomes leaves and a dash becomes dashes, and no rule gets all of them right.
    /// </summary>
    public MeasurementUnitSE(string name, long value, string abbreviation, string? singularAbbreviation = null) : base(name, value)
    {
        this.Abbreviation = abbreviation;
        this.SingularAbbreviation = singularAbbreviation ?? abbreviation;
    }

    #endregion Constructors

    #region Properties

    /// <summary>
    /// How the unit reads beside an amount on a card or a shopping list.
    /// </summary>
    public string Abbreviation { get; } = string.Empty;

    /// <summary>
    /// How the unit reads beside exactly one — "1 packet", never "1 packets".
    /// </summary>
    public string SingularAbbreviation { get; } = string.Empty;

    #endregion Properties

    #region Methods

    public static implicit operator MeasurementUnitSE(string name)
        => FromName<MeasurementUnitSE>(name) ?? throw new ArgumentException($"'{name}' is not a recognised {nameof(MeasurementUnitSE)} name.", nameof(name));

    public static implicit operator MeasurementUnitSE(long value)
        => FromValue<MeasurementUnitSE>(value) ?? throw new ArgumentException($"'{value}' is not a recognised {nameof(MeasurementUnitSE)} value.", nameof(value));

    #endregion Methods

}
