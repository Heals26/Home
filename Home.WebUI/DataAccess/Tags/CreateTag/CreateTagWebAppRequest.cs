namespace Home.WebUI.DataAccess.Tags.CreateTag;

public class CreateTagWebAppRequest
{

    #region Properties

    /// <summary>
    /// A hex colour in the form #RRGGBB.
    /// </summary>
    public string Colour { get; set; } = string.Empty;

    /// <summary>
    /// The name of the label.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    #endregion Properties

}
