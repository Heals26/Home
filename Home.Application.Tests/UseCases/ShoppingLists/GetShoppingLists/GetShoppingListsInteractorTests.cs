using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.ShoppingLists.GetShoppingLists;
using Home.Domain.Entities;
using Home.WebApi.Presenters.ShoppingLists.GetShoppingLists;
using Home.WebApi.UseCases.ShoppingLists.GetShoppingLists;

namespace Home.Application.Tests.UseCases.ShoppingLists.GetShoppingLists;

/// <summary>
/// The list picker. Every row carries a count of what is on it, which is the only reason the
/// items are projected at all — get that wrong and every list reads as empty before it is opened.
/// </summary>
public class GetShoppingListsInteractorTests : InteractorTest
{

    #region Fields

    private readonly GetShoppingListsPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private static ShoppingList BuildList(long shoppingListID, Household household, string name, int itemCount, bool isArchived = false)
    {
        var _List = new ShoppingList()
        {
            Household = household,
            IsArchived = isArchived,
            Name = name,
            ShoppingListID = shoppingListID
        };

        _List.Items =
        [
            .. Enumerable.Range(1, itemCount).Select(i => new ShoppingListItem()
            {
                Name = $"Item {i}",
                Sequence = i,
                ShoppingList = _List,
                ShoppingListItemID = shoppingListID + i
            })
        ];

        return _List;
    }

    private Task HandleAsync()
        => new GetShoppingListsInteractor().HandleAsync(
            new GetShoppingListsInputPort(),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_CountsWhatIsOnEachList()
    {
        _ = this.Database.Seed(
            BuildList(120, this.Ours, "This week", itemCount: 3),
            BuildList(140, this.Ours, "Party", itemCount: 0));

        await this.HandleAsync();

        var _Lists = Ok<GetShoppingListsApiResponse>(this.m_Presenter).ShoppingLists;

        _ = _Lists.Single(l => l.Name == "This week").ItemCount.Should().Be(
            3,
            "an unprojected item collection counts zero and every list reads as empty");
        _ = _Lists.Single(l => l.Name == "Party").ItemCount.Should().Be(0);
    }

    [Fact]
    public async Task HandleAsync_ReturnsOurListsInAStableOrderAndNobodyElses()
    {
        _ = this.Database.Seed(
            BuildList(140, this.Ours, "Party", itemCount: 0),
            BuildList(120, this.Ours, "Christmas", itemCount: 1),
            BuildList(920, this.Theirs, "Anniversary", itemCount: 1));

        await this.HandleAsync();

        _ = Ok<GetShoppingListsApiResponse>(this.m_Presenter).ShoppingLists
            .Select(l => l.Name).Should().Equal(["Christmas", "Party"]);
    }

    [Fact]
    public async Task HandleAsync_StillReturnsAnArchivedListSoThePickerCanDecideWhatToHide()
    {
        _ = this.Database.Seed(
            BuildList(120, this.Ours, "Christmas", itemCount: 1, isArchived: true),
            BuildList(140, this.Ours, "Party", itemCount: 0));

        await this.HandleAsync();

        _ = Ok<GetShoppingListsApiResponse>(this.m_Presenter).ShoppingLists
            .Single(l => l.Name == "Christmas").IsArchived.Should().BeTrue(
                "archiving takes a list out of the picker, it does not take it out of the data");
    }

    #endregion Methods

}
