using FluentAssertions;
using Home.Application.Infrastructure.ShoppingLists;
using Home.Application.Services.EntityLogic.ShoppingLists;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.ShoppingListItems.CreateShoppingListItem;
using Home.Domain.Entities;
using Home.Domain.Enumerations;
using Home.Domain.Services.Audits;
using Home.WebApi.Presenters.ShoppingListItems.CreateShoppingListItem;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Home.Application.Tests.UseCases.ShoppingListItems.CreateShoppingListItem;

/// <summary>
/// Adding something to a shopping list. The position is counted from what is already on the list,
/// which is deliberately a count rather than the highest number, so a deletion reuses a position.
/// </summary>
public class CreateShoppingListItemInteractorTests : InteractorTest
{

    #region Fields

    private readonly Mock<IAuditLogic<ShoppingList>> m_AuditLogic = new();
    private readonly CreateShoppingListItemPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private static ShoppingList BuildList(long shoppingListID, Household household, params (long ItemID, string Name, long Sequence)[] items)
    {
        var _List = new ShoppingList()
        {
            Household = household,
            Name = $"List {shoppingListID}",
            ShoppingListID = shoppingListID
        };

        _List.Items =
        [
            .. items.Select(i => new ShoppingListItem()
            {
                Name = i.Name,
                Sequence = i.Sequence,
                ShoppingList = _List,
                ShoppingListItemID = i.ItemID
            })
        ];

        return _List;
    }

    private Task HandleAsync(long shoppingListID, string name, decimal? amount = null, decimal? cost = null, long? unit = null, bool inBasket = false)
    {
        var _Services = this.Services(out var _Context);

        return new CreateShoppingListItemInteractor().HandleAsync(
            new CreateShoppingListItemInputPort(amount, cost, inBasket, name, shoppingListID, unit),
            this.m_Presenter,
            _Services
                .With(this.m_AuditLogic.Object)
                .With<IShoppingListLogic>(new ShoppingListLogic(_Context))
                .Build(),
            CancellationToken.None);
    }

    [Fact]
    public async Task HandleAsync_AddsTheItemToTheList()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours));

        await this.HandleAsync(120, "Milk", amount: 2, cost: 4.80m, unit: MeasurementUnitSE.Litres.Value);

        _ = this.m_Presenter.Result.Should().BeOfType<CreatedResult>();

        var _Stored = this.Stored<ShoppingListItem>().Single();

        _ = _Stored.Name.Should().Be("Milk");
        _ = _Stored.Amount.Should().Be(2);
        _ = _Stored.Cost.Should().Be(4.80m);
        _ = _Stored.Unit.Should().Be(MeasurementUnitSE.Litres.Value);
        _ = _Stored.InBasket.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_PutsANewItemOnTheEnd()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours, (130, "Bread", 1), (131, "Eggs", 2)));

        await this.HandleAsync(120, "Milk");

        _ = this.Stored<ShoppingListItem>().Single(i => i.Name == "Milk").Sequence.Should().Be(3);
    }

    [Fact]
    public async Task HandleAsync_OnAnEmptyListStartsAtOne()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours));

        await this.HandleAsync(120, "Milk");

        _ = this.Stored<ShoppingListItem>().Single().Sequence.Should().Be(1);
    }

    [Fact]
    public async Task HandleAsync_RecordsThatTheListChanged()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours));

        await this.HandleAsync(120, "Milk");

        this.m_AuditLogic.Verify(a => a.UpdateAudit(It.IsAny<ShoppingList>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenTheListBelongsToAnotherHousehold_PresentsNotFoundAndAddsNothing()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours), BuildList(920, this.Theirs));

        await this.HandleAsync(920, "Added by us");

        ShouldBeNotFound(this.m_Presenter);
        _ = this.Stored<ShoppingListItem>().Should().BeEmpty();
    }

    #endregion Methods

}
