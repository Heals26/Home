using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Domain.Entities;

namespace Home.Application.UseCases.ActivityStates.GetActivityStates;

internal class GetActivityStatesInteractor : IInteractor<GetActivityStatesInputPort, IGetActivityStatesOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        GetActivityStatesInputPort input,
        IGetActivityStatesOutputPort output,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();

        var _ActivityStates = _PersistenceContext.GetEntities<ActivityState>()
            .OrderBy(s => s.ActivityStateID)
            .ToList();

        await output.PresentActivityStatesAsync(_ActivityStates, cancellationToken);
    }

    #endregion Methods

}
