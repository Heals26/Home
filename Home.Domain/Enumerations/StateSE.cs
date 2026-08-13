namespace Home.Domain.Enumerations;

public class StateSE : BaseEnumeration
{

    #region Constructors

    public StateSE(string name, long value) : base(name, value) { }

    #endregion Constructors

    #region Methods

    public static implicit operator StateSE(string name)
        => FromName<StateSE>(name) ?? throw new ArgumentException($"'{name}' is not a recognised {nameof(StateSE)} name.", nameof(name));

    public static implicit operator StateSE(long value)
        => FromValue<StateSE>(value) ?? throw new ArgumentException($"'{value}' is not a recognised {nameof(StateSE)} value.", nameof(value));

    #endregion Methods

}
