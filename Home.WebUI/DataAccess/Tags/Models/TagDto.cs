namespace Home.WebUI.DataAccess.Tags.Models;

public class TagDto
{

    #region Properties

    /// <summary>
    /// A validated #RRGGBB value, safe to drop into an inline style.
    /// </summary>
    public string Colour { get; set; } = string.Empty;

    /// <summary>
    /// The name of the label.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The ID of the label.
    /// </summary>
    public long TagID { get; set; }

    #endregion Properties

}
