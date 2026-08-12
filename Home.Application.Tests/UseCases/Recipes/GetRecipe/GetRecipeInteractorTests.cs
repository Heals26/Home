using FluentAssertions;
using Home.Application.Services.Persistence;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.Recipes.GetRecipe;
using Home.Domain.Entities;
using Moq;

namespace Home.Application.Tests.UseCases.Recipes.GetRecipe;

public class GetRecipeInteractorTests
{

    #region Fields

    private readonly Mock<IPersistenceContext> m_PersistenceContext = new();
    private readonly Mock<IGetRecipeOutputPort> m_OutputPort = new();

    #endregion Fields

    #region Methods

    private static Recipe BuildRecipe(long recipeID, string name = "Spaghetti Bolognese")
        => new()
        {
            RecipeID = recipeID,
            Name = name,
            Url = "https://example.test/bolognese",
            Ingredients = [],
            Notes = [],
            Steps = []
        };

    private Task HandleAsync(long requestedRecipeID, params Recipe[] stored)
    {
        _ = this.m_PersistenceContext
            .Setup(c => c.GetEntities<Recipe>())
            .Returns(stored.AsQueryable());

        var _ServiceFactory = new TestServiceFactory()
            .With(this.m_PersistenceContext.Object)
            .Build();

        return new GetRecipeInteractor().HandleAsync(
            new GetRecipeInputPort(requestedRecipeID),
            this.m_OutputPort.Object,
            _ServiceFactory,
            CancellationToken.None);
    }

    [Fact]
    public async Task HandleAsync_WhenTheRecipeExists_PresentsThatRecipe()
    {
        var _Recipe = BuildRecipe(7);

        await this.HandleAsync(7, BuildRecipe(6, "Carbonara"), _Recipe);

        this.m_OutputPort.Verify(
            o => o.PresentRecipeAsync(
                It.Is<Recipe>(r => r.RecipeID == 7 && r.Name == "Spaghetti Bolognese"),
                It.IsAny<CancellationToken>()),
            Times.Once);

        this.m_OutputPort.Verify(
            o => o.PresentRecipeNotFoundAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenTheRecipeDoesNotExist_PresentsNotFoundWithTheRequestedID()
    {
        await this.HandleAsync(99, BuildRecipe(6), BuildRecipe(7));

        this.m_OutputPort.Verify(
            o => o.PresentRecipeNotFoundAsync(99, It.IsAny<CancellationToken>()),
            Times.Once);

        this.m_OutputPort.Verify(
            o => o.PresentRecipeAsync(It.IsAny<Recipe>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenNoRecipesExistAtAll_PresentsNotFound()
    {
        await this.HandleAsync(1);

        this.m_OutputPort.Verify(
            o => o.PresentRecipeNotFoundAsync(1, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenTheRecipeExists_DoesNotWriteToThePersistenceContext()
    {
        await this.HandleAsync(7, BuildRecipe(7));

        this.m_PersistenceContext.Verify(
            c => c.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_PassesTheCancellationTokenThroughToTheOutputPort()
    {
        using var _Source = new CancellationTokenSource();

        _ = this.m_PersistenceContext
            .Setup(c => c.GetEntities<Recipe>())
            .Returns(new[] { BuildRecipe(7) }.AsQueryable());

        await new GetRecipeInteractor().HandleAsync(
            new GetRecipeInputPort(7),
            this.m_OutputPort.Object,
            new TestServiceFactory().With(this.m_PersistenceContext.Object).Build(),
            _Source.Token);

        this.m_OutputPort.Verify(
            o => o.PresentRecipeAsync(It.IsAny<Recipe>(), _Source.Token),
            Times.Once);
    }

    #endregion Methods

}
