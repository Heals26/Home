namespace Home.Domain;


public static class DomainValues
{
    private const int SpacesBetweenNames = 2;
    private const int BaseNameLength = 50;

    #region Fields

    public static string Schema = "home";
    public static int FirstNameLength = BaseNameLength;
    public static int MiddleNameLength = BaseNameLength;
    public static int LastNameLength = BaseNameLength;
    public static int FullNameLength = FirstNameLength + MiddleNameLength + LastNameLength + SpacesBetweenNames;

    #endregion Fields

}
