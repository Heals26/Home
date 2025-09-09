namespace Home.Domain.Enumerations;

public class ResourceTypeSE : BaseEnumeration
{

    #region Fields

    public static ResourceTypeSE User = new("User", 1);
    public static ResourceTypeSE Activity = new("Activity", 2);
    public static ResourceTypeSE Recipe = new("Recipe", 3);
    public static ResourceTypeSE Note = new("Note", 4);

    #endregion Fields

    #region Constructors

    public ResourceTypeSE(string name, long value) : base(name, value) { }

    #endregion Constructors

    #region Methods

    public static implicit operator ResourceTypeSE(string name)
        => FromName<ResourceTypeSE>(name);

    public static implicit operator ResourceTypeSE(long value)
        => FromValue<ResourceTypeSE>(value);

    #endregion Methods

}
