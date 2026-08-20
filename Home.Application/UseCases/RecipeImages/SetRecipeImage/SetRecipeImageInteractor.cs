using CleanArchitecture.Mediator;
using Home.Application.Infrastructure.Recipes;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.RecipeImages.SetRecipeImage;

/// <summary>
/// Stores the household's photo of the dish, replacing whatever photo was there before.
/// </summary>
internal class SetRecipeImageInteractor : IInteractor<SetRecipeImageInputPort, ISetRecipeImageOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        SetRecipeImageInputPort inputPort,
        ISetRecipeImageOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();
        var _Now = serviceFactory.GetService<TimeProvider>().GetUtcNow().UtcDateTime;

        var _Household = _AuthorisationService.GetHousehold();

        var _Recipe = _PersistenceContext.GetEntities<Recipe>()
            .SingleOrDefault(r => r.RecipeID == inputPort.RecipeID
                && r.Household.HouseholdID == _Household.HouseholdID);

        if (_Recipe == null)
        {
            await outputPort.PresentRecipeNotFoundAsync(inputPort.RecipeID, cancellationToken);
            return;
        }

        var _Image = _PersistenceContext.GetEntities<RecipeImage>()
            .SingleOrDefault(i => i.Recipe.RecipeID == inputPort.RecipeID);

        if (_Image == null)
        {
            _Image = new RecipeImage() { Recipe = _Recipe };
            _PersistenceContext.Add(_Image);
        }

        // The validator has already proven the bytes are a drawable image, so the detect cannot
        // come back null here.
        _Image.Content = inputPort.Content;
        _Image.ContentType = RecipeImageLogic.DetectContentType(inputPort.Content)!;

        // The recipe row carries the timestamp so the book can say "has a photo" cheaply, and
        // its ticks bust the image URL's cache.
        _Recipe.ImageUpdatedOnUTC = _Now;

        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentRecipeImageSetNoContentAsync(cancellationToken);
    }

    #endregion Methods

}
