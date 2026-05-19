namespace Home.Domain;

public static class DomainValues
{
    #region Constants

    private const int SpacesBetweenNames = 2;
    private const int BaseNameLength = 50;

    #endregion Constants

    #region Fields

    public static readonly string Schema = "home";
    public static readonly int FirstNameLength = BaseNameLength;
    public static readonly int MiddleNameLength = BaseNameLength;
    public static readonly int LastNameLength = BaseNameLength;
    public static readonly int FullNameLength = FirstNameLength + MiddleNameLength + LastNameLength + SpacesBetweenNames;

    #endregion Fields

}
