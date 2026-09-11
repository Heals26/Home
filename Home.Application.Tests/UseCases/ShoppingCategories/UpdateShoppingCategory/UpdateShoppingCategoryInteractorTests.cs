using FluentAssertions;
using Home.Application.Infrastructure.ChangeTrackers;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.ShoppingCategories.UpdateShoppingCategory;
using Home.Domain.Entities;
using Home.WebApi.Presenters.ShoppingCategories.UpdateShoppingCategory;
using Microsoft.AspNetCore.Mvc;

namespace Home.Application.Tests.UseCases.ShoppingCategories.UpdateShoppingCategory;

/// <summary>
/// Renaming an aisle, or moving it along the household's walk through the shop.
/// </summary>
public class UpdateShoppingCategoryInteractorTests : InteractorTest
{

    #region Fields

    private readonly UpdateShoppingCategoryPresenter m_Presenter = new(Mapper);

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

    private Task HandleAsync(
        long shoppingCategoryID,
        PropertyChangeTracker<string> name = default,
        PropertyChangeTracker<int> sequence = default)
        => new UpdateShoppingCategoryInteractor().HandleAsync(
            new UpdateShoppingCategoryInputPort(name, sequence, shoppingCategoryID),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_RenamesTheAisle()
    {
        _ = this.Database.Seed(BuildCategory(110, this.Ours, "Dairy", 1));

        await this.HandleAsync(110, name: new("  Dairy and eggs  "));

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();
        _ = this.Stored<ShoppingCategory>().Single().Name.Should().Be("Dairy and eggs");
    }

    [Fact]
    public async Task HandleAsync_MovesTheAisleWithoutTouchingItsName()
    {
        _ = this.Database.Seed(BuildCategory(110, this.Ours, "Dairy", 1));

        await this.HandleAsync(110, sequence: new(4));

        var _Stored = this.Stored<ShoppingCategory>().Single();

        _ = _Stored.Sequence.Should().Be(4);
        _ = _Stored.Name.Should().Be("Dairy");
    }

    [Fact]
    public async Task HandleAsync_LetsAnAisleChangeTheCaseOfItsOwnName()
    {
        _ = this.Database.Seed(BuildCategory(110, this.Ours, "dairy", 1));

        await this.HandleAsync(110, name: new("Dairy"));

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>("an aisle cannot clash with itself");
        _ = this.Stored<ShoppingCategory>().Single().Name.Should().Be("Dairy");
    }

    [Fact]
    public async Task HandleAsync_WhenAnotherOfOurAislesHasThatNameInAnyCase_Refuses()
    {
        _ = this.Database.Seed(BuildCategory(110, this.Ours, "Dairy", 0), BuildCategory(111, this.Ours, "Frozen", 1));

        await this.HandleAsync(111, name: new("DAIRY"));

        _ = this.m_Presenter.Result.Should().BeOfType<ConflictResult>();
        _ = this.Stored<ShoppingCategory>().Single(c => c.ShoppingCategoryID == 111).Name.Should().Be("Frozen");
    }

    [Fact]
    public async Task HandleAsync_AllowsANameAnotherHouseholdUses()
    {
        _ = this.Database.Seed(BuildCategory(110, this.Ours, "Frozen", 0), BuildCategory(910, this.Theirs, "Dairy", 0));

        await this.HandleAsync(110, name: new("Dairy"));

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task HandleAsync_WhenTheAisleBelongsToAnotherHousehold_PresentsNotFoundAndLeavesIt()
    {
        _ = this.Database.Seed(BuildCategory(110, this.Ours, "Dairy", 0), BuildCategory(910, this.Theirs, "Deli", 0));

        await this.HandleAsync(910, name: new("Renamed by us"));

        ShouldBeNotFound(this.m_Presenter);
        _ = this.Stored<ShoppingCategory>().Single(c => c.ShoppingCategoryID == 910).Name.Should().Be("Deli");
    }

    #endregion Methods

}
