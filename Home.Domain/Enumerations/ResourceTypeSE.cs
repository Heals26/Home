namespace Home.Domain.Enumerations;

public class ResourceTypeSE : BaseEnumeration
{

    #region Fields

    public static ResourceTypeSE User = new("User", 1);
    public static ResourceTypeSE Activity = new("Activity", 2);
    public static ResourceTypeSE Recipe = new("Recipe", 3);
    public static ResourceTypeSE Note = new("Note", 4);
    public static ResourceTypeSE ShoppingCart = new("ShoppingCart", 5);

    #endregion Fields

    #region Constructors

    public ResourceTypeSE(string name, long value) : base(name, value) { }

    #endregion Constructors

    #region Methods

    public static implicit operator ResourceTypeSE(string name)
        => FromName<ResourceTypeSE>(name) ?? throw new ArgumentException($"'{name}' is not a recognised {nameof(ResourceTypeSE)} name.", nameof(name));

    public static implicit operator ResourceTypeSE(long value)
        => FromValue<ResourceTypeSE>(value) ?? throw new ArgumentException($"'{value}' is not a recognised {nameof(ResourceTypeSE)} value.", nameof(value));

    #endregion Methods

}
