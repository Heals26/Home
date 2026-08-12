using FluentAssertions;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.Recipes.CreateRecipe;
using Home.Domain.Entities;
using Moq;

namespace Home.Application.Tests.UseCases.Recipes.CreateRecipe;

public class CreateRecipeInteractorTests
{

    #region Fields

    private readonly Mock<IPersistenceContext> m_PersistenceContext = new();
    private readonly Mock<IAuthorisationService> m_AuthorisationService = new();
    private readonly Mock<ICreateRecipeOutputPort> m_OutputPort = new();
    private readonly Household m_Household = new() { HouseholdID = 42 };

    #endregion Fields

    #region Methods

    private Task HandleAsync(string name = "Pad Thai", string url = "https://example.test/pad-thai")
    {
        _ = this.m_AuthorisationService.Setup(a => a.GetHousehold()).Returns(this.m_Household);

        var _ServiceFactory = new TestServiceFactory()
            .With(this.m_PersistenceContext.Object)
            .With(this.m_AuthorisationService.Object)
            .Build();

        return new CreateRecipeInteractor().HandleAsync(
            new CreateRecipeInputPort(name, url),
            this.m_OutputPort.Object,
            _ServiceFactory,
            CancellationToken.None);
    }

    [Fact]
    public async Task HandleAsync_AddsARecipeCarryingTheInputPortValues()
    {
        Recipe? _Added = null;
        this.m_PersistenceContext
            .Setup(c => c.Add(It.IsAny<Recipe>()))
            .Callback<Recipe>(r => _Added = r);

        await this.HandleAsync("Pad Thai", "https://example.test/pad-thai");

        _Added.Should().NotBeNull();
        _Added!.Name.Should().Be("Pad Thai");
        _Added.Url.Should().Be("https://example.test/pad-thai");
    }

    [Fact]
    public async Task HandleAsync_AssignsTheHouseholdFromTheAuthorisationService()
    {
        Recipe? _Added = null;
        this.m_PersistenceContext
            .Setup(c => c.Add(It.IsAny<Recipe>()))
            .Callback<Recipe>(r => _Added = r);

        await this.HandleAsync();

        _Added!.Household.Should().BeSameAs(this.m_Household);
    }

    [Fact]
    public async Task HandleAsync_InitialisesTheChildCollectionsSoTheyAreNeverNull()
    {
        Recipe? _Added = null;
        this.m_PersistenceContext
            .Setup(c => c.Add(It.IsAny<Recipe>()))
            .Callback<Recipe>(r => _Added = r);

        await this.HandleAsync();

        _Added!.Ingredients.Should().NotBeNull().And.BeEmpty();
        _Added.Notes.Should().NotBeNull().And.BeEmpty();
        _Added.Steps.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_SavesChangesExactlyOnce()
    {
        await this.HandleAsync();

        this.m_PersistenceContext.Verify(
            c => c.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_PresentsTheIDAssignedDuringSave()
    {
        // The database assigns the key on save, so the presented ID must be read after
        // SaveChangesAsync rather than before it.
        Recipe? _Added = null;
        this.m_PersistenceContext
            .Setup(c => c.Add(It.IsAny<Recipe>()))
            .Callback<Recipe>(r => _Added = r);
        this.m_PersistenceContext
            .Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => _Added!.RecipeID = 123)
            .ReturnsAsync(1);

        await this.HandleAsync();

        this.m_OutputPort.Verify(
            o => o.PresentRecipeCreatedAsync(123, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion Methods

}
