using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.Tags.GetTags;

internal class GetTagsInteractor : IInteractor<GetTagsInputPort, IGetTagsOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        GetTagsInputPort inputPort,
        IGetTagsOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();

        var _Household = _AuthorisationService.GetHousehold();

        var _Tags = _PersistenceContext.GetEntities<Tag>()
            .Where(t => t.Household.HouseholdID == _Household.HouseholdID)
            .OrderBy(t => t.Name)
            .ToList();

        await outputPort.PresentTagsAsync(_Tags, cancellationToken);
    }

    #endregion Methods

}
