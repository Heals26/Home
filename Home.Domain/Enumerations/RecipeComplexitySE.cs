namespace Home.Domain.Enumerations;

public class RecipeComplexitySE : BaseEnumeration
{

    #region Fields

    public static RecipeComplexitySE Easy = new("Easy", 1);
    public static RecipeComplexitySE Moderate = new("Moderate", 2);
    public static RecipeComplexitySE Involved = new("Involved", 3);

    #endregion Fields

    #region Constructors

    public RecipeComplexitySE(string name, long value) : base(name, value) { }

    #endregion Constructors

    #region Methods

    public static implicit operator RecipeComplexitySE(string name)
        => FromName<RecipeComplexitySE>(name) ?? throw new ArgumentException($"'{name}' is not a recognised {nameof(RecipeComplexitySE)} name.", nameof(name));

    public static implicit operator RecipeComplexitySE(long value)
        => FromValue<RecipeComplexitySE>(value) ?? throw new ArgumentException($"'{value}' is not a recognised {nameof(RecipeComplexitySE)} value.", nameof(value));

    #endregion Methods

}
