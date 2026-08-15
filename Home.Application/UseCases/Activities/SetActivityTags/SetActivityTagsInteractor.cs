using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.Activities.SetActivityTags;

internal class SetActivityTagsInteractor
    : IInteractor<SetActivityTagsInputPort, ISetActivityTagsOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        SetActivityTagsInputPort inputPort,
        ISetActivityTagsOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();

        var _Household = _AuthorisationService.GetHousehold();

        var _Activity = _PersistenceContext.GetEntities<Activity>()
            .Where(a => a.ActivityID == inputPort.ActivityID
                && a.Household.HouseholdID == _Household.HouseholdID)
            .Select(a => new
            {
                Activity = a,
                a.Tags
            })
            .SingleOrDefault()
            ?.Activity;

        if (_Activity == null)
        {
            await outputPort.PresentActivityNotFoundAsync(inputPort.ActivityID, cancellationToken);
            return;
        }

        List<long> _TagIDs = inputPort.TagIDs == null ? [] : [.. inputPort.TagIDs.Distinct()];

        var _Tags = _PersistenceContext.GetEntities<Tag>()
            .Where(t => _TagIDs.Contains(t.TagID)
                && t.Household.HouseholdID == _Household.HouseholdID)
            .ToList();

        // Anything that did not come back belongs to another family or does not exist, and either
        // way it must not end up on this card.
        var _UnknownTagIDs = _TagIDs.Except(_Tags.Select(t => t.TagID)).ToList();

        if (_UnknownTagIDs.Count != 0)
        {
            await outputPort.PresentTagNotFoundAsync(_UnknownTagIDs[0], cancellationToken);
            return;
        }

        var _Removed = _Activity.Tags
            .Where(t => !_TagIDs.Contains(t.TagID))
            .ToList();

        _PersistenceContext.RemoveRange(_Removed);

        foreach (var _Tag in _Tags.Where(t => !_Activity.Tags.Any(at => at.TagID == t.TagID)))
            _PersistenceContext.Add(new ActivityTag()
            {
                Activity = _Activity,
                Tag = _Tag
            });

        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentActivityTagsSetAsync(cancellationToken);
    }

    #endregion Methods

}
