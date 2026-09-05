using FluentAssertions;
using Home.Application.Infrastructure.ChangeTrackers;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.ShoppingLists.UpdateShoppingList;
using Home.Domain.Entities;
using Home.WebApi.Presenters.ShoppingLists.UpdateShoppingList;
using Microsoft.AspNetCore.Mvc;

namespace Home.Application.Tests.UseCases.ShoppingLists.UpdateShoppingList;

/// <summary>
/// Renaming a list or putting it away. Archiving keeps everything and only takes the list out of
/// the picker, which is what separates it from deleting.
/// </summary>
public class UpdateShoppingListInteractorTests : InteractorTest
{

    #region Fields

    private readonly UpdateShoppingListPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private static ShoppingList BuildList(long shoppingListID, Household household, string name, bool isArchived = false)
        => new()
        {
            Household = household,
            IsArchived = isArchived,
            Items = [new ShoppingListItem() { Name = "Milk", Sequence = 1, ShoppingListItemID = shoppingListID + 1 }],
            Name = name,
            ShoppingListID = shoppingListID
        };

    private Task HandleAsync(long shoppingListID, PropertyChangeTracker<bool> isArchived = default, PropertyChangeTracker<string> name = default)
        => new UpdateShoppingListInteractor().HandleAsync(
            new UpdateShoppingListInputPort(isArchived, name, shoppingListID),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_RenamesTheListAndSavesIt()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours, "This week"));

        await this.HandleAsync(120, name: new("Christmas"));

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();
        _ = this.Stored<ShoppingList>().Single().Name.Should().Be("Christmas");
    }

    [Fact]
    public async Task HandleAsync_ArchivingKeepsEverythingOnTheList()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours, "Christmas"));

        await this.HandleAsync(120, isArchived: new(true));

        _ = this.Stored<ShoppingList>().Single().IsArchived.Should().BeTrue();
        _ = this.Stored<ShoppingListItem>().Should().ContainSingle(
            "archiving only takes a list out of the picker, which is what makes it different from deleting");
    }

    [Fact]
    public async Task HandleAsync_CanBringAnArchivedListBack()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours, "Christmas", isArchived: true));

        await this.HandleAsync(120, isArchived: new(false));

        _ = this.Stored<ShoppingList>().Single().IsArchived.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_WhenOnlyTheNameIsSent_LeavesTheArchiveFlagAlone()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours, "Christmas", isArchived: true));

        await this.HandleAsync(120, name: new("Last Christmas"));

        _ = this.Stored<ShoppingList>().Single().IsArchived.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WhenTheListBelongsToAnotherHousehold_ChangesNothing()
    {
        _ = this.Database.Seed(BuildList(920, this.Theirs, "Theirs"));

        await this.HandleAsync(920, name: new("Renamed by us"));

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();
        _ = this.Stored<ShoppingList>().Single().Name.Should().Be("Theirs");
    }

    #endregion Methods

}
