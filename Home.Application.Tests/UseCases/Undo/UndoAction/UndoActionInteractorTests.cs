using CleanArchitecture.Mediator;
using FluentAssertions;
using Home.Application.Infrastructure.Activities;
using Home.Application.Infrastructure.ChangeTrackers;
using Home.Application.Infrastructure.ShoppingLists;
using Home.Application.Infrastructure.Undo;
using Home.Application.Infrastructure.Values;
using Home.Application.Services.EntityLogic.Activities;
using Home.Application.Services.EntityLogic.ShoppingLists;
using Home.Application.Services.Undo;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.Activities.SetActivityCompletion;
using Home.Application.UseCases.RecipeIngredients.RemoveRecipeIngredient;
using Home.Application.UseCases.Recipes.DeleteRecipe;
using Home.Application.UseCases.ShoppingCategories.DeleteShoppingCategory;
using Home.Application.UseCases.ShoppingListItems.DeleteShoppingListItem;
using Home.Application.UseCases.ShoppingListItems.SetShoppingListItemInBasket;
using Home.Application.UseCases.ShoppingListItems.UpdateShoppingListItem;
using Home.Application.UseCases.ShoppingLists.DeleteShoppingList;
using Home.Application.UseCases.ShoppingLists.DeleteTickedShoppingListItems;
using Home.Application.UseCases.ShoppingLists.EndShoppingTrip;
using Home.Application.UseCases.ShoppingLists.UntickShoppingListItems;
using Home.Application.UseCases.Undo.UndoAction;
using Home.Application.UseCases.Users.DeleteUser;
using Home.Domain.Entities;
using Home.Persistence.Undo;
using Home.WebApi.Presenters.Activities.SetActivityCompletion;
using Home.WebApi.Presenters.RecipeIngredients.RemoveRecipeIngredient;
using Home.WebApi.Presenters.Recipes.DeleteRecipe;
using Home.WebApi.Presenters.ShoppingCategories.DeleteShoppingCategory;
using Home.WebApi.Presenters.ShoppingListItems.DeleteShoppingListItem;
using Home.WebApi.Presenters.ShoppingListItems.SetShoppingListItemInBasket;
using Home.WebApi.Presenters.ShoppingListItems.UpdateShoppingListItem;
using Home.WebApi.Presenters.ShoppingLists.DeleteShoppingList;
using Home.WebApi.Presenters.ShoppingLists.DeleteTickedShoppingListItems;
using Home.WebApi.Presenters.ShoppingLists.EndShoppingTrip;
using Home.WebApi.Presenters.ShoppingLists.UntickShoppingListItems;
using Home.WebApi.Presenters.Undo.UndoAction;
using Home.WebApi.Presenters.Users.DeleteUser;
using Microsoft.AspNetCore.Mvc;

namespace Home.Application.Tests.UseCases.Undo.UndoAction;

/// <summary>
/// The Undo on the bar (17 Sep): ticks and deletes anywhere, put back exactly, for the device that
/// made the request and nobody else, and only while the undo is still honoured.
/// </summary>
public class UndoActionInteractorTests : InteractorTest
{

    #region Fields

    private readonly UndoActionPresenter m_Presenter = new(Mapper);
    private readonly Guid m_Token = Guid.NewGuid();

    /// <summary>
    /// How long after <see cref="TestServiceFactory.DefaultNow"/> the next request happens.
    /// </summary>
    private TimeSpan m_Elapsed;

    #endregion Fields

    #region Properties

    private static DateTime NowUTC
        => TestServiceFactory.DefaultNow.UtcDateTime;

    #endregion Properties

    #region Methods

    /// <summary>
    /// Makes a request the way a device offering Undo does: with the token, for whoever is signed in.
    /// </summary>
    private Task ActAsync<TInputPort, TOutputPort>(IInteractor<TInputPort, TOutputPort> interactor, TInputPort inputPort, TOutputPort outputPort)
        where TInputPort : IInputPort<TOutputPort>
    {
        var _Services = this.Services(
            out var _Context,
            new UndoScope() { HouseholdID = this.SignedInHousehold.HouseholdID, Token = this.m_Token });

        _Services.Time.Advance(this.m_Elapsed);

        return interactor.HandleAsync(
            inputPort,
            outputPort,
            _Services
                .With<IActivityLogic>(new ActivityLogic(_Context, _Services.Time))
                .With<IShoppingItemMemoryLogic>(new ShoppingItemMemoryLogic(_Context, _Services.Time))
                .With<IShoppingListLogic>(new ShoppingListLogic(_Context))
                .With<IShoppingTripLogic>(new ShoppingTripLogic(_Context, _Services.Time))
                .Build(),
            CancellationToken.None);
    }

    private static ShoppingList BuildList(long shoppingListID, Household household, params (long ItemID, string Name, bool InBasket, long? TripID)[] items)
    {
        var _List = new ShoppingList()
        {
            Household = household,
            Name = $"List {shoppingListID}",
            ShoppingListID = shoppingListID
        };

        _List.Items =
        [
            .. items.Select((i, index) => new ShoppingListItem()
            {
                Amount = 2,
                Cost = 4.80m,
                InBasket = i.InBasket,
                Name = i.Name,
                Sequence = index + 1,
                ShoppingList = _List,
                ShoppingListItemID = i.ItemID,
                ShoppingTripID = i.TripID
            })
        ];

        return _List;
    }

    private static ShoppingTrip BuildTrip(long shoppingTripID, ShoppingList shoppingList)
        => new()
        {
            LastActivityOnUTC = NowUTC,
            ShoppingList = shoppingList,
            ShoppingTripID = shoppingTripID,
            StartedOnUTC = NowUTC
        };

    private Task UndoAsync(UndoActionPresenter? presenter = null)
    {
        var _Services = this.Services(out var _Context);

        _Services.Time.Advance(this.m_Elapsed);

        return new UndoActionInteractor().HandleAsync(
            new UndoActionInputPort(this.m_Token),
            presenter ?? this.m_Presenter,
            _Services.With<IUndoStore>(new UndoStore(_Context, _Services.Time)).Build(),
            CancellationToken.None);
    }

    [Fact]
    public async Task HandleAsync_BringsBackALineRemovedFromAList()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours, (130, "Milk", false, null)));

        await this.ActAsync(new DeleteShoppingListItemInteractor(), new DeleteShoppingListItemInputPort(130), new DeleteShoppingListItemPresenter(Mapper));

        _ = this.Stored<ShoppingListItem>().Should().BeEmpty("a held-back delete is gone as far as any query can tell");

        await this.UndoAsync();

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();
        _ = this.Stored<ShoppingListItem>().Select(i => i.ShoppingListItemID).Should().Equal([130], "the same line comes back, not a copy of it");
    }

    [Fact]
    public async Task HandleAsync_TakesBackATickAndThePurchaseItRecorded()
    {
        var _List = BuildList(120, this.Ours, (130, "Milk", false, null));

        _List.Trips = [BuildTrip(150, _List)];

        _ = this.Database.Seed(_List);

        await this.ActAsync(
            new SetShoppingListItemInBasketInteractor(),
            new SetShoppingListItemInBasketInputPort(true, 130),
            new SetShoppingListItemInBasketPresenter(Mapper));

        _ = this.Stored<ShoppingItemPrice>().Should().ContainSingle();

        await this.UndoAsync();

        var _Item = this.Stored<ShoppingListItem>().Single();

        _ = _Item.InBasket.Should().BeFalse();
        _ = _Item.ShoppingTripID.Should().BeNull();
        _ = this.Stored<ShoppingItemPrice>().Should().BeEmpty("the purchase the tick recorded goes with the tick");
        _ = this.Stored<ShoppingItemMemory>().Should().ContainSingle("the memory of the item is shared by every list, so it stays");
    }

    [Fact]
    public async Task HandleAsync_BringsBackThePurchaseAnUntickTookBack()
    {
        var _List = BuildList(120, this.Ours, (130, "Milk", true, 150));
        var _Memory = new ShoppingItemMemory() { Household = this.Ours, Name = "Milk", NameKey = "milk", ShoppingItemMemoryID = 140 };

        _List.Trips = [BuildTrip(150, _List)];
        _Memory.Prices = [new ShoppingItemPrice() { BoughtOnUTC = NowUTC, Cost = 4.80m, Memory = _Memory, ShoppingItemPriceID = 160, ShoppingListItemID = 130, ShoppingTripID = 150 }];

        _ = this.Database.Seed(_Memory, _List);

        await this.ActAsync(
            new SetShoppingListItemInBasketInteractor(),
            new SetShoppingListItemInBasketInputPort(false, 130),
            new SetShoppingListItemInBasketPresenter(Mapper));

        _ = this.Stored<ShoppingItemPrice>().Should().BeEmpty();

        await this.UndoAsync();

        var _Item = this.Stored<ShoppingListItem>().Single();

        _ = _Item.InBasket.Should().BeTrue();
        _ = _Item.ShoppingTripID.Should().Be(150);
        _ = this.Stored<ShoppingItemPrice>().Select(p => p.ShoppingItemPriceID).Should().Equal([160]);
    }

    [Fact]
    public async Task HandleAsync_PutsClearedItemsBackAndReopensTheShop()
    {
        var _List = BuildList(120, this.Ours, (130, "Milk", true, 150), (131, "Bread", false, null));

        _List.Trips = [BuildTrip(150, _List)];

        _ = this.Database.Seed(_List);

        await this.ActAsync(new DeleteTickedShoppingListItemsInteractor(), new DeleteTickedShoppingListItemsInputPort(120), new DeleteTickedShoppingListItemsPresenter(Mapper));

        _ = this.Stored<ShoppingListItem>().Should().ContainSingle();
        _ = this.Stored<ShoppingTrip>().Single().EndedOnUTC.Should().NotBeNull();

        await this.UndoAsync();

        _ = this.Stored<ShoppingListItem>().Select(i => i.ShoppingListItemID).Should().BeEquivalentTo([130, 131]);
        _ = this.Stored<ShoppingTrip>().Single().EndedOnUTC.Should().BeNull("clearing the trolley ended the shop, so undoing it opens the shop again");
    }

    [Fact]
    public async Task HandleAsync_PutsEverythingBackInTheTrolleyAfterUntickAll()
    {
        var _List = BuildList(120, this.Ours, (130, "Milk", true, 150), (131, "Bread", false, null));

        _List.Trips = [BuildTrip(150, _List)];

        _ = this.Database.Seed(_List);

        await this.ActAsync(new UntickShoppingListItemsInteractor(), new UntickShoppingListItemsInputPort(120), new UntickShoppingListItemsPresenter(Mapper));
        await this.UndoAsync();

        var _Milk = this.Stored<ShoppingListItem>().Single(i => i.ShoppingListItemID == 130);

        _ = _Milk.InBasket.Should().BeTrue();
        _ = _Milk.ShoppingTripID.Should().Be(150);
        _ = this.Stored<ShoppingListItem>().Single(i => i.ShoppingListItemID == 131).InBasket.Should().BeFalse();
        _ = this.Stored<ShoppingTrip>().Single().EndedOnUTC.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_ReopensAShopEndedByDone()
    {
        var _List = BuildList(120, this.Ours);

        _List.Trips = [BuildTrip(150, _List)];

        _ = this.Database.Seed(_List);

        await this.ActAsync(new EndShoppingTripInteractor(), new EndShoppingTripInputPort(120), new EndShoppingTripPresenter(Mapper));
        await this.UndoAsync();

        _ = this.Stored<ShoppingTrip>().Single().EndedOnUTC.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_BringsBackADeletedListWithEverythingOnIt()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours, (130, "Milk", false, null)));

        await this.ActAsync(new DeleteShoppingListInteractor(), new DeleteShoppingListInputPort(120), new DeleteShoppingListPresenter(Mapper));

        _ = this.Stored<ShoppingList>().Should().BeEmpty();
        _ = this.Stored<ShoppingListItem>().Count(i => i.ShoppingList.ShoppingListID == 120).Should().Be(0, "a line cannot be reached past a list that is held back");

        await this.UndoAsync();

        _ = this.Stored<ShoppingList>().Should().ContainSingle();
        _ = this.Stored<ShoppingListItem>().Count(i => i.ShoppingList.ShoppingListID == 120).Should().Be(1);
    }

    [Fact]
    public async Task HandleAsync_FilesItemsUnderAnAisleAgainWhenItComesBack()
    {
        var _Dairy = new ShoppingCategory() { Household = this.Ours, Name = "Dairy", ShoppingCategoryID = 110 };

        _ = this.Database.Seed(new ShoppingItemMemory() { Household = this.Ours, Name = "Milk", NameKey = "milk", ShoppingCategory = _Dairy, ShoppingItemMemoryID = 140 });

        await this.ActAsync(new DeleteShoppingCategoryInteractor(), new DeleteShoppingCategoryInputPort(110), new DeleteShoppingCategoryPresenter(Mapper));

        _ = this.Stored<ShoppingItemMemory>().Count(m => m.ShoppingCategory != null).Should().Be(0);

        await this.UndoAsync();

        _ = this.Stored<ShoppingCategory>().Should().ContainSingle();
        _ = this.Stored<ShoppingItemMemory>().Count(m => m.ShoppingCategory != null && m.ShoppingCategory.ShoppingCategoryID == 110).Should().Be(1);
    }

    [Fact]
    public async Task HandleAsync_BringsBackAMemberOnTheirEventsAndChores()
    {
        var _Bo = new User() { UserID = 102, FirstName = "Bo", Household = this.Ours, LastName = "Member" };

        _ = this.Database.Seed(
            new Activity() { ActivityID = 150, Household = this.Ours, Title = "Bins", User = _Bo },
            new CalendarEvent() { CalendarEventID = 160, Household = this.Ours, Title = "Swimming", StartDate = new DateOnly(2026, 9, 10), EndDate = new DateOnly(2026, 9, 10), IsAllDay = true, Members = [new CalendarEventMember() { User = _Bo }] });

        await this.ActAsync(new DeleteUserInteractor(), new DeleteUserInputPort(102), new DeleteUserPresenter(Mapper));

        _ = this.Stored<User>().Any(u => u.UserID == 102).Should().BeFalse();
        _ = this.Stored<CalendarEventMember>().Should().BeEmpty();

        await this.UndoAsync();

        _ = this.Stored<User>().Any(u => u.UserID == 102).Should().BeTrue();
        _ = this.Stored<Activity>().Count(a => a.User != null && a.User.UserID == 102).Should().Be(1, "the chore names them again");
        _ = this.Stored<CalendarEventMember>().Should().ContainSingle("they are back on the event");
    }

    [Fact]
    public async Task HandleAsync_BringsBackAnIngredientRemovedFromARecipe()
    {
        var _Recipe = new Recipe() { Household = this.Ours, Name = "Pancakes", RecipeID = 120 };

        _Recipe.Ingredients = [new RecipeIngredient() { Ingredient = new Ingredient() { IngredientID = 130, Name = "Flour" }, Recipe = _Recipe, Sequence = 1 }];

        _ = this.Database.Seed(_Recipe);

        await this.ActAsync(new RemoveRecipeIngredientInteractor(), new RemoveRecipeIngredientInputPort(130, 120), new RemoveRecipeIngredientPresenter(Mapper));

        _ = this.Stored<RecipeIngredient>().Should().BeEmpty();

        await this.UndoAsync();

        _ = this.Stored<RecipeIngredient>().Should().ContainSingle();
        _ = this.Stored<Ingredient>().Select(i => i.IngredientID).Should().Equal([130]);
    }

    [Fact]
    public async Task HandleAsync_HidesAPlannedMealWithItsRecipeUntilTheDeleteIsUndone()
    {
        var _Recipe = new Recipe() { Household = this.Ours, Name = "Pancakes", RecipeID = 120 };

        _ = this.Database.Seed(new MealPlanEntry() { Date = NowUTC.Date, Household = this.Ours, MealPlanEntryID = 150, Recipe = _Recipe });

        await this.ActAsync(new DeleteRecipeInteractor(), new DeleteRecipeInputPort(120), new DeleteRecipePresenter(Mapper));

        _ = this.Stored<MealPlanEntry>().Should().BeEmpty("the purge's cascade will take the planned meal with the recipe, so it goes now");

        await this.UndoAsync();

        _ = this.Stored<Recipe>().Should().ContainSingle();
        _ = this.Stored<MealPlanEntry>().Should().ContainSingle();
    }

    [Fact]
    public async Task HandleAsync_TakesBackTickingOffAChore()
    {
        _ = this.Database.Seed(this.Member);

        var _ToDo = new ActivityState() { ActivityStateID = 120, Household = this.Ours, Name = "To do", Sequence = 0 };

        _ = this.Database.Seed(
            new ActivityState() { ActivityStateID = 121, Household = this.Ours, IsComplete = true, Name = "Done", Sequence = 1 },
            new Activity() { ActivityID = 150, Household = this.Ours, State = _ToDo, Title = "Bins" });

        await this.ActAsync(new SetActivityCompletionInteractor(), new SetActivityCompletionInputPort(150, true), new SetActivityCompletionPresenter(Mapper));

        _ = this.Stored<Activity>().Single().CompletedDateUTC.Should().NotBeNull();

        await this.UndoAsync();

        _ = this.Stored<Activity>().Count(a => a.State != null && a.State.ActivityStateID == 120).Should().Be(1);
        _ = this.Stored<Activity>().Single().CompletedDateUTC.Should().BeNull();
        _ = this.Stored<Activity>().Count(a => a.CompletedByUser != null).Should().Be(0);
    }

    [Fact]
    public async Task HandleAsync_LeavesAloneWhatWasChangedAgainSince()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours, (130, "Milk", false, null)));

        await this.ActAsync(
            new UpdateShoppingListItemInteractor(),
            new UpdateShoppingListItemInputPort(default, new PropertyChangeTracker<decimal?>(5.20m), default, default, 130, default),
            new UpdateShoppingListItemPresenter(Mapper));

        var _Services = this.Services(out var _Context);

        await new UpdateShoppingListItemInteractor().HandleAsync(
            new UpdateShoppingListItemInputPort(default, new PropertyChangeTracker<decimal?>(6.00m), default, default, 130, default),
            new UpdateShoppingListItemPresenter(Mapper),
            _Services
                .With<IShoppingItemMemoryLogic>(new ShoppingItemMemoryLogic(_Context, _Services.Time))
                .With<IShoppingListLogic>(new ShoppingListLogic(_Context))
                .With<IShoppingTripLogic>(new ShoppingTripLogic(_Context, _Services.Time))
                .Build(),
            CancellationToken.None);

        await this.UndoAsync();

        _ = this.Stored<ShoppingListItem>().Single().Cost.Should().Be(6.00m, "another device changed the price since, and that change stands");
    }

    [Fact]
    public async Task HandleAsync_WhenTheUndoHasRunOut_ChangesNothing()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours, (130, "Milk", false, null)));

        await this.ActAsync(new DeleteShoppingListItemInteractor(), new DeleteShoppingListItemInputPort(130), new DeleteShoppingListItemPresenter(Mapper));

        this.m_Elapsed = UndoValues.HonouredFor + TimeSpan.FromSeconds(1);
        await this.UndoAsync();

        _ = this.m_Presenter.Result.Should().BeOfType<ConflictResult>();
        _ = this.Stored<ShoppingListItem>().Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_OnlyUndoesOnce()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours, (130, "Milk", false, null)));

        await this.ActAsync(new DeleteShoppingListItemInteractor(), new DeleteShoppingListItemInputPort(130), new DeleteShoppingListItemPresenter(Mapper));
        await this.UndoAsync();

        var _Again = new UndoActionPresenter(Mapper);

        await this.UndoAsync(_Again);

        ShouldBeNotFound(_Again);
        _ = this.Stored<ShoppingListItem>().Should().ContainSingle();
    }

    [Fact]
    public async Task HandleAsync_WhenTheRequestWasAnotherHouseholds_FindsNothingToUndo()
    {
        _ = this.Database.Seed(this.Neighbour, BuildList(920, this.Theirs, (930, "Caviar", false, null)));

        this.SignedInHousehold = this.Theirs;
        this.SignedInUser = this.Neighbour;

        await this.ActAsync(new DeleteShoppingListItemInteractor(), new DeleteShoppingListItemInputPort(930), new DeleteShoppingListItemPresenter(Mapper));

        this.SignedInHousehold = this.Ours;
        this.SignedInUser = this.Member;

        await this.UndoAsync();

        ShouldBeNotFound(this.m_Presenter);
        _ = this.Stored<ShoppingListItem>().Should().BeEmpty("their line stays deleted, whoever holds the token");
    }

    [Fact]
    public async Task HandleAsync_WhenTheRequestDidSomethingThatCannotBeHeldBack_PutsNothingBack()
    {
        var _List = BuildList(120, this.Ours);

        _List.Trips = [BuildTrip(150, _List)];

        _ = this.Database.Seed(_List);

        var _Scope = new UndoScope() { HouseholdID = OurHouseholdID, Token = this.m_Token };
        var _Context = this.Database.Read(_Scope);

        _Context.Remove(_Context.GetEntities<ShoppingTrip>().Single());
        _ = await _Context.SaveChangesAsync(CancellationToken.None);

        await this.UndoAsync();

        _ = this.m_Presenter.Result.Should().BeOfType<ConflictResult>();
        _ = this.Stored<ShoppingTrip>().Should().BeEmpty();
    }

    #endregion Methods

}
