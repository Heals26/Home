using Home.Domain.Entities;

namespace Home.Application.UseCases.Tags.GetTags;

public interface IGetTagsOutputPort
{

    #region Methods

    Task PresentTagsAsync(IEnumerable<Tag> tags, CancellationToken cancellationToken);

    #endregion Methods

}
