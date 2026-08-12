namespace Home.WebUI.DataAccess.Lights.Models;

public class LightGroupDto
{

    #region Properties

    /// <summary>
    /// Home's group ID, used to address group-wide commands.
    /// </summary>
    public long LightGroupID { get; set; }

    /// <summary>
    /// The lights in this group, ordered by name.
    /// </summary>
    public List<LightDto> Lights { get; set; } = [];

    /// <summary>
    /// The group's name, editable in Home.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Display order on the Lights page.
    /// </summary>
    public int Sequence { get; set; }

    #endregion Properties

}
