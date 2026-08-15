namespace Home.Application.UseCases.Tags.Models;

public static class TagValues
{

    #region Fields

    /// <summary>
    /// A tag colour is interpolated into an inline style on the board, so only a plain six-digit
    /// hex value is ever allowed as far as the database.
    /// </summary>
    public const string ColourPattern = "^#[0-9A-Fa-f]{6}$";

    #endregion Fields

}
