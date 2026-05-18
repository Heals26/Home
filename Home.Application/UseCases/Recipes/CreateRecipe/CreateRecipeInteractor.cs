using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.Recipes.CreateRecipe;

internal class CreateRecipeInteractor : IInteractor<CreateRecipeInputPort, ICreateRecipeOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        CreateRecipeInputPort inputPort,
        ICreateRecipeOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();

        var _Recipe = new Recipe()
        {
            Household = _AuthorisationService.GetHousehold(),
            Ingredients = [],
            Name = inputPort.Name,
            Notes = [],
            Steps = [],
            Url = inputPort.Url
        };

        _PersistenceContext.Add(_Recipe);
        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentRecipeCreatedAsync(_Recipe.RecipeID, cancellationToken);
    }

    #endregion Methods

}
