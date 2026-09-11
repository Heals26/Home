using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.ShoppingCategories.CreateShoppingCategory;
using Home.Domain.Entities;
using Home.WebApi.Presenters.ShoppingCategories.CreateShoppingCategory;
using Microsoft.AspNetCore.Mvc;

namespace Home.Application.Tests.UseCases.ShoppingCategories.CreateShoppingCategory;

/// <summary>
/// Adding an aisle to the household's shop. "Dairy" and "dairy" are the same aisle.
/// </summary>
public class CreateShoppingCategoryInteractorTests : InteractorTest
{

    #region Fields

    private readonly CreateShoppingCategoryPresenter m_Presenter = new(Mapper);

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

    private Task HandleAsync(string name)
        => new CreateShoppingCategoryInteractor().HandleAsync(
            new CreateShoppingCategoryInputPort(name),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_AddsTheAisleAtTheEndOfTheSignedInHouseholdsShop()
    {
        _ = this.Database.Seed(BuildCategory(110, this.Ours, "Dairy", 0), BuildCategory(111, this.Ours, "Frozen", 1));

        await this.HandleAsync("  Deli  ");

        _ = this.m_Presenter.Result.Should().BeOfType<CreatedResult>();
        _ = this.Stored<ShoppingCategory>()
            .Count(c => c.Household.HouseholdID == OurHouseholdID && c.Name == "Deli" && c.Sequence == 2)
            .Should().Be(1);
    }

    [Fact]
    public async Task HandleAsync_OnAHouseholdWithNoAislesStartsAtZero()
    {
        _ = this.Database.Seed(this.Ours);

        await this.HandleAsync("Deli");

        _ = this.Stored<ShoppingCategory>().Single().Sequence.Should().Be(0);
    }

    [Fact]
    public async Task HandleAsync_PlacesTheAisleAfterOurOwnAislesOnly()
    {
        _ = this.Database.Seed(BuildCategory(110, this.Ours, "Dairy", 0), BuildCategory(910, this.Theirs, "Frozen", 7));

        await this.HandleAsync("Deli");

        _ = this.Stored<ShoppingCategory>().Single(c => c.Name == "Deli").Sequence.Should().Be(1);
    }

    [Fact]
    public async Task HandleAsync_WhenTheHouseholdAlreadyHasThatAisleInAnyCase_Refuses()
    {
        _ = this.Database.Seed(BuildCategory(110, this.Ours, "Dairy", 0));

        await this.HandleAsync("dairy");

        _ = this.m_Presenter.Result.Should().BeOfType<ConflictResult>();
        _ = this.Stored<ShoppingCategory>().Should().ContainSingle();
    }

    [Fact]
    public async Task HandleAsync_AllowsAnAisleNameAnotherHouseholdUses()
    {
        _ = this.Database.Seed(BuildCategory(910, this.Theirs, "Dairy", 0));

        await this.HandleAsync("Dairy");

        _ = this.m_Presenter.Result.Should().BeOfType<CreatedResult>();
        _ = this.Stored<ShoppingCategory>().Should().HaveCount(2);
    }

    #endregion Methods

}
