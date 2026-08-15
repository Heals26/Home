using FluentAssertions;
using Home.Application.Services.EntityLogic.Recipes;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.ShoppingLists.AddMealPlanToShoppingList;
using Home.Domain.Entities;
using Moq;

namespace Home.Application.Tests.UseCases.ShoppingLists.AddMealPlanToShoppingList;

public class AddMealPlanToShoppingListInteractorTests
{

    #region Fields

    private readonly Mock<IPersistenceContext> m_PersistenceContext = new();
    private readonly Mock<IAuthorisationService> m_AuthorisationService = new();
    private readonly Mock<IRecipeLogic> m_RecipeLogic = new();
    private readonly Mock<IAddMealPlanToShoppingListOutputPort> m_OutputPort = new();
    private readonly Household m_Household = new() { HouseholdID = 42 };

    #endregion Fields

    #region Methods

    private Task HandleAsync(long shoppingListID, ShoppingList[] shoppingLists, MealPlanEntry[] entries)
    {
        _ = this.m_AuthorisationService.Setup(a => a.GetHousehold()).Returns(this.m_Household);
        _ = this.m_PersistenceContext.Setup(c => c.GetEntities<ShoppingList>()).Returns(shoppingLists.AsQueryable());
        _ = this.m_PersistenceContext.Setup(c => c.GetEntities<MealPlanEntry>()).Returns(entries.AsQueryable());

        var _ServiceFactory = new TestServiceFactory()
            .With(this.m_PersistenceContext.Object)
            .With(this.m_AuthorisationService.Object)
            .With(this.m_RecipeLogic.Object)
            .Build();

        return new AddMealPlanToShoppingListInteractor().HandleAsync(
            new AddMealPlanToShoppingListInputPort(new DateTime(2026, 8, 17), null, shoppingListID, new DateTime(2026, 8, 23)),
            this.m_OutputPort.Object,
            _ServiceFactory,
            CancellationToken.None);
    }

    [Fact]
    public async Task HandleAsync_PresentsNotFoundForAnotherHouseholdsList()
    {
        await this.HandleAsync(
            5,
            [new ShoppingList() { ShoppingListID = 5, Household = new Household() { HouseholdID = 999 }, Items = [] }],
            []);

        this.m_OutputPort.Verify(
            o => o.PresentShoppingListNotFoundAsync(5, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_AddsARecipePlannedTwiceInTheWeekOnlyOnce()
    {
        var _Recipe = new Recipe() { RecipeID = 7, Household = this.m_Household, Ingredients = [] };
        var _List = new ShoppingList() { ShoppingListID = 5, Household = this.m_Household, Items = [] };

        await this.HandleAsync(
            5,
            [_List],
            [
                new MealPlanEntry() { MealPlanEntryID = 1, Date = new DateTime(2026, 8, 17), Recipe = _Recipe },
                new MealPlanEntry() { MealPlanEntryID = 2, Date = new DateTime(2026, 8, 19), Recipe = _Recipe }
            ]);

        this.m_RecipeLogic.Verify(l => l.AddIngredientsToShoppingList(_Recipe, _List, null), Times.Once);
        this.m_OutputPort.Verify(
            o => o.PresentMealPlanAddedToShoppingListAsync(1, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_IgnoresEntriesOutsideTheDateWindow()
    {
        var _Recipe = new Recipe() { RecipeID = 7, Household = this.m_Household, Ingredients = [] };
        var _List = new ShoppingList() { ShoppingListID = 5, Household = this.m_Household, Items = [] };

        await this.HandleAsync(
            5,
            [_List],
            [new MealPlanEntry() { MealPlanEntryID = 1, Date = new DateTime(2026, 8, 30), Recipe = _Recipe }]);

        this.m_RecipeLogic.Verify(
            l => l.AddIngredientsToShoppingList(It.IsAny<Recipe>(), It.IsAny<ShoppingList>(), It.IsAny<IReadOnlyCollection<long>?>()),
            Times.Never);
        this.m_OutputPort.Verify(
            o => o.PresentMealPlanAddedToShoppingListAsync(0, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion Methods

}
