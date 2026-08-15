using Home.WebUI.DataAccess.Tags.Models;

namespace Home.WebUI.DataAccess.Tags.GetTags;

public class GetTagsWebAppResponse
{

    #region Properties

    /// <summary>
    /// The household's labels.
    /// </summary>
    public ICollection<TagDto> Tags { get; set; } = [];

    #endregion Properties

}
