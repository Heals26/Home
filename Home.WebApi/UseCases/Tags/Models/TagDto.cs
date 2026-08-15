namespace Home.WebApi.UseCases.Tags.Models;

public class TagDto
{

    #region Properties

    /// <summary>
    /// A validated #RRGGBB value, safe to drop into an inline style.
    /// </summary>
    public string Colour { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public long TagID { get; set; }

    #endregion Properties

}
