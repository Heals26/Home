using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Domain.Entities;

namespace Home.Application.UseCases.RecipeSteps.RemoveRecipeStep;

internal class RemoveRecipeStepInteractor : IInteractor<RemoveRecipeStepInputPort, IRemoveRecipeStepOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        RemoveRecipeStepInputPort inputPort,
        IRemoveRecipeStepOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();

        var _Step = _PersistenceContext.Find<RecipeStep>(inputPort.RecipeStepID);

        if (_Step == null)
        {
            await outputPort.PresentRecipeStepNotFoundAsync(inputPort.RecipeStepID, cancellationToken);
            return;
        }

        _PersistenceContext.Remove(_Step);
        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentRecipeStepRemovedAsync(cancellationToken);
    }

    #endregion Methods

}
