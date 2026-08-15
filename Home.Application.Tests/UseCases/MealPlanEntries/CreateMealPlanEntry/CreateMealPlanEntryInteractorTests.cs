using FluentAssertions;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.MealPlanEntries.CreateMealPlanEntry;
using Home.Domain.Entities;
using Moq;

namespace Home.Application.Tests.UseCases.MealPlanEntries.CreateMealPlanEntry;

public class CreateMealPlanEntryInteractorTests
{

    #region Fields

    private readonly Mock<IPersistenceContext> m_PersistenceContext = new();
    private readonly Mock<IAuthorisationService> m_AuthorisationService = new();
    private readonly Mock<ICreateMealPlanEntryOutputPort> m_OutputPort = new();
    private readonly Household m_Household = new() { HouseholdID = 42 };

    #endregion Fields

    #region Methods

    private Task HandleAsync(DateTime date, long recipeID, params Recipe[] recipes)
    {
        _ = this.m_AuthorisationService.Setup(a => a.GetHousehold()).Returns(this.m_Household);
        _ = this.m_PersistenceContext.Setup(c => c.GetEntities<Recipe>()).Returns(recipes.AsQueryable());

        var _ServiceFactory = new TestServiceFactory()
            .With(this.m_PersistenceContext.Object)
            .With(this.m_AuthorisationService.Object)
            .Build();

        return new CreateMealPlanEntryInteractor().HandleAsync(
            new CreateMealPlanEntryInputPort(date, null, recipeID),
            this.m_OutputPort.Object,
            _ServiceFactory,
            CancellationToken.None);
    }

    [Fact]
    public async Task HandleAsync_NormalisesTheDateToMidnight()
    {
        MealPlanEntry? _Added = null;
        this.m_PersistenceContext
            .Setup(c => c.Add(It.IsAny<MealPlanEntry>()))
            .Callback<MealPlanEntry>(e => _Added = e);

        await this.HandleAsync(
            new DateTime(2026, 8, 17, 18, 45, 12),
            recipeID: 7,
            new Recipe() { RecipeID = 7, Household = this.m_Household });

        _Added.Should().NotBeNull();
        _Added!.Date.Should().Be(new DateTime(2026, 8, 17));
    }

    [Fact]
    public async Task HandleAsync_PresentsNotFoundForAnotherHouseholdsRecipe()
    {
        await this.HandleAsync(
            new DateTime(2026, 8, 17),
            recipeID: 7,
            new Recipe() { RecipeID = 7, Household = new Household() { HouseholdID = 999 } });

        this.m_OutputPort.Verify(
            o => o.PresentRecipeNotFoundAsync(7, It.IsAny<CancellationToken>()),
            Times.Once);
        this.m_PersistenceContext.Verify(c => c.Add(It.IsAny<MealPlanEntry>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_PresentsTheIDAssignedDuringSave()
    {
        MealPlanEntry? _Added = null;
        this.m_PersistenceContext
            .Setup(c => c.Add(It.IsAny<MealPlanEntry>()))
            .Callback<MealPlanEntry>(e => _Added = e);
        this.m_PersistenceContext
            .Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => _Added!.MealPlanEntryID = 55)
            .ReturnsAsync(1);

        await this.HandleAsync(
            new DateTime(2026, 8, 17),
            recipeID: 7,
            new Recipe() { RecipeID = 7, Household = this.m_Household });

        this.m_OutputPort.Verify(
            o => o.PresentMealPlanEntryCreatedAsync(55, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion Methods

}
