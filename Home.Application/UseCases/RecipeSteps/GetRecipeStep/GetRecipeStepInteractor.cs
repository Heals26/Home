using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Domain.Entities;

namespace Home.Application.UseCases.RecipeSteps.GetRecipeStep;

internal class GetRecipeStepInteractor : IInteractor<GetRecipeStepInputPort, IGetRecipeStepOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        GetRecipeStepInputPort inputPort,
        IGetRecipeStepOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();

        var _Step = _PersistenceContext.Find<RecipeStep>(inputPort.RecipeStepID);

        if (_Step == null)
            await outputPort.PresentRecipeStepNotFoundAsync(inputPort.RecipeStepID, cancellationToken);
        else
            await outputPort.PresentRecipeStepAsync(_Step, cancellationToken);
    }

    #endregion Methods

}
