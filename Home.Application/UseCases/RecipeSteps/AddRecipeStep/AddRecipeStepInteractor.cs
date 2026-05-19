using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Domain.Entities;

namespace Home.Application.UseCases.RecipeSteps.AddRecipeStep;

internal class AddRecipeStepInteractor : IInteractor<AddRecipeStepInputPort, IAddRecipeStepOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        AddRecipeStepInputPort inputPort,
        IAddRecipeStepOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();

        var _Recipe = _PersistenceContext.GetEntities<Recipe>()
            .Where(r => r.RecipeID == inputPort.RecipeID)
            .Select(r => new
            {
                Recipe = r,
                r.Steps
            })
            .SingleOrDefault()
            ?.Recipe;

        if (_Recipe == null)
        {
            await outputPort.PresentRecipeNotFoundAsync(inputPort.RecipeID, cancellationToken);
            return;
        }

        var _Step = new RecipeStep()
        {
            Content = inputPort.Content,
            Sequence = inputPort.Sequence,
            Title = inputPort.Title
        };

        _Recipe.Steps.Add(_Step);
        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentRecipeStepAddedAsync(_Step.RecipeStepID, cancellationToken);
    }

    #endregion Methods

}
