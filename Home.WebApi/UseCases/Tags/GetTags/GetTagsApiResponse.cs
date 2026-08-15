using Home.WebApi.UseCases.Tags.Models;

namespace Home.WebApi.UseCases.Tags.GetTags;

public class GetTagsApiResponse
{

    #region Properties

    public ICollection<TagDto> Tags { get; set; } = [];

    #endregion Properties

}
