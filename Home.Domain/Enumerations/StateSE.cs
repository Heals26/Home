namespace Home.Domain.Enumerations;

public class StateSE : BaseEnumeration
{

    #region Constructors

    public StateSE(string name, long value) : base(name, value) { }

    #endregion Constructors

    #region Methods

    public static implicit operator StateSE(string name)
        => FromName<StateSE>(name);

    public static implicit operator StateSE(long value)
        => FromValue<StateSE>(value);

    #endregion Methods

}
