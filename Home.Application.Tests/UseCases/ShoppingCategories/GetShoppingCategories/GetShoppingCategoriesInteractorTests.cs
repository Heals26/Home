using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.ShoppingCategories.GetShoppingCategories;
using Home.Domain.Entities;
using Home.WebApi.Presenters.ShoppingCategories.GetShoppingCategories;
using Home.WebApi.UseCases.ShoppingCategories.GetShoppingCategories;

namespace Home.Application.Tests.UseCases.ShoppingCategories.GetShoppingCategories;

/// <summary>
/// The household's aisles, in the order it walks the shop.
/// </summary>
public class GetShoppingCategoriesInteractorTests : InteractorTest
{

    #region Fields

    private readonly GetShoppingCategoriesPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private static ShoppingCategory BuildCategory(long shoppingCategoryID, Household household, string name, int sequence)
        => new()
        {
            Household = household,
            Name = name,
            Sequence = sequence,
            ShoppingCategoryID = shoppingCategoryID
        };

    private Task HandleAsync()
        => new GetShoppingCategoriesInteractor().HandleAsync(
            new GetShoppingCategoriesInputPort(),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_BringsBackOurAislesInTheOrderWeWalkThem()
    {
        _ = this.Database.Seed(
            BuildCategory(110, this.Ours, "Frozen", 1),
            BuildCategory(111, this.Ours, "Fruit and veg", 0),
            BuildCategory(910, this.Theirs, "Deli", 0));

        await this.HandleAsync();

        _ = Ok<GetShoppingCategoriesApiResponse>(this.m_Presenter).ShoppingCategories
            .Select(c => c.Name)
            .Should().Equal(["Fruit and veg", "Frozen"], "another household's aisles are never ours");
    }

    #endregion Methods

}
