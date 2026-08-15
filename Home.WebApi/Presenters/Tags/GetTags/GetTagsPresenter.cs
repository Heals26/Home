using AutoMapper;
using Home.Application.UseCases.Tags.GetTags;
using Home.Domain.Entities;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.Tags.GetTags;
using Home.WebApi.UseCases.Tags.Models;

namespace Home.WebApi.Presenters.Tags.GetTags;

public class GetTagsPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IGetTagsOutputPort
{

    #region Methods

    Task IGetTagsOutputPort.PresentTagsAsync(IEnumerable<Tag> tags, CancellationToken cancellationToken)
        => this.OkAsync(new GetTagsApiResponse()
        {
            Tags = [.. tags.Select(t => new TagDto()
            {
                Colour = t.Colour,
                Name = t.Name,
                TagID = t.TagID
            })]
        }, cancellationToken);

    #endregion Methods

}
