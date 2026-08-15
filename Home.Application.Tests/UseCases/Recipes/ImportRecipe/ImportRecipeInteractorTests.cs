using FluentAssertions;
using Home.Application.Services.Persistence;
using Home.Application.Services.RecipeImports;
using Home.Application.Services.Security;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.Recipes.ImportRecipe;
using Home.Domain.Entities;
using Moq;

namespace Home.Application.Tests.UseCases.Recipes.ImportRecipe;

public class ImportRecipeInteractorTests
{

    #region Fields

    private readonly Mock<IPersistenceContext> m_PersistenceContext = new();
    private readonly Mock<IAuthorisationService> m_AuthorisationService = new();
    private readonly Mock<IRecipeImportService> m_RecipeImportService = new();
    private readonly Mock<IImportRecipeOutputPort> m_OutputPort = new();
    private readonly Household m_Household = new() { HouseholdID = 42 };

    #endregion Fields

    #region Methods

    private Task HandleAsync(string url, ImportedRecipe? imported)
    {
        _ = this.m_AuthorisationService.Setup(a => a.GetHousehold()).Returns(this.m_Household);
        _ = this.m_RecipeImportService
            .Setup(s => s.FetchRecipeAsync(url, It.IsAny<CancellationToken>()))
            .ReturnsAsync(imported);

        var _ServiceFactory = new TestServiceFactory()
            .With(this.m_PersistenceContext.Object)
            .With(this.m_AuthorisationService.Object)
            .With(this.m_RecipeImportService.Object)
            .Build();

        return new ImportRecipeInteractor().HandleAsync(
            new ImportRecipeInputPort(url),
            this.m_OutputPort.Object,
            _ServiceFactory,
            CancellationToken.None);
    }

    [Fact]
    public async Task HandleAsync_PresentsFailureWhenThePageHasNoRecipe()
    {
        await this.HandleAsync("https://example.test/not-a-recipe", null);

        this.m_OutputPort.Verify(
            o => o.PresentRecipeImportFailedAsync("https://example.test/not-a-recipe", It.IsAny<CancellationToken>()),
            Times.Once);
        this.m_PersistenceContext.Verify(c => c.Add(It.IsAny<Recipe>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_BuildsTheRecipeGraphFromTheImport()
    {
        Recipe? _Added = null;
        this.m_PersistenceContext
            .Setup(c => c.Add(It.IsAny<Recipe>()))
            .Callback<Recipe>(r => _Added = r);

        await this.HandleAsync(
            "https://example.test/pad-thai",
            new ImportedRecipe(
                null,
                null,
                ["200 g rice noodles", "2 eggs"],
                "Pad Thai",
                null,
                null,
                [new ImportedRecipeStep("Prep", "Soak the noodles."), new ImportedRecipeStep(string.Empty, "Fry everything.")]));

        _Added.Should().NotBeNull();
        _Added!.Name.Should().Be("Pad Thai");
        _Added.Url.Should().Be("https://example.test/pad-thai");
        _Added.Household.Should().BeSameAs(this.m_Household);
        _Added.Ingredients.Select(i => i.Ingredient.Name).Should().Equal("200 g rice noodles", "2 eggs");
        _Added.Steps.OrderBy(s => s.Sequence).Select(s => s.Content).Should().Equal("Soak the noodles.", "Fry everything.");
        _Added.Steps.OrderBy(s => s.Sequence).Select(s => s.Sequence).Should().Equal(1, 2);
    }

    [Fact]
    public async Task HandleAsync_PresentsTheIDAssignedDuringSave()
    {
        Recipe? _Added = null;
        this.m_PersistenceContext
            .Setup(c => c.Add(It.IsAny<Recipe>()))
            .Callback<Recipe>(r => _Added = r);
        this.m_PersistenceContext
            .Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => _Added!.RecipeID = 321)
            .ReturnsAsync(1);

        await this.HandleAsync(
            "https://example.test/pad-thai",
            new ImportedRecipe(null, null, ["Noodles"], "Pad Thai", null, null, []));

        this.m_OutputPort.Verify(
            o => o.PresentRecipeImportedAsync(321, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion Methods

}
