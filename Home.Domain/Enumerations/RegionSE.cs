namespace Home.Domain.Enumerations;

public class RegionSE : BaseEnumeration
{

    #region Fields

    public static RegionSE Description = new("Description", 1);
    public static RegionSE AcceptanceCriteria = new("AcceptanceCriteria", 2);
    public static RegionSE Notes = new("Notes", 3);

    #endregion Fields

    #region Constructors

    public RegionSE(string name, long value) : base(name, value) { }

    #endregion Constructors

    #region Methods

    public static implicit operator RegionSE(string name)
        => FromName<RegionSE>(name) ?? throw new ArgumentException($"'{name}' is not a recognised {nameof(RegionSE)} name.", nameof(name));

    public static implicit operator RegionSE(long value)
        => FromValue<RegionSE>(value) ?? throw new ArgumentException($"'{value}' is not a recognised {nameof(RegionSE)} value.", nameof(value));

    #endregion Methods

}
