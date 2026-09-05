using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.ShoppingLists.CreateShoppingList;
using Home.Domain.Entities;
using Home.Domain.Services.Audits;
using Home.WebApi.Presenters.ShoppingLists.CreateShoppingList;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Home.Application.Tests.UseCases.ShoppingLists.CreateShoppingList;

/// <summary>
/// Starting a new shopping list.
/// </summary>
public class CreateShoppingListInteractorTests : InteractorTest
{

    #region Fields

    private readonly Mock<IAuditLogic<ShoppingList>> m_AuditLogic = new();
    private readonly CreateShoppingListPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private Task HandleAsync(string name)
        => new CreateShoppingListInteractor().HandleAsync(
            new CreateShoppingListInputPort(name),
            this.m_Presenter,
            this.Services().With(this.m_AuditLogic.Object).Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_WritesTheListToTheSignedInHousehold()
    {
        _ = this.Database.Seed(this.Ours);

        await this.HandleAsync("This week");

        _ = this.m_Presenter.Result.Should().BeOfType<CreatedResult>();

        var _Stored = this.Stored<ShoppingList>().Single();

        _ = _Stored.Name.Should().Be("This week");
        _ = _Stored.IsArchived.Should().BeFalse("a brand new list is not put away");
        _ = this.Stored<ShoppingList>().Count(sl => sl.Household.HouseholdID == OurHouseholdID).Should().Be(1);
    }

    [Fact]
    public async Task HandleAsync_StartsTheListEmpty()
    {
        _ = this.Database.Seed(this.Ours);

        await this.HandleAsync("This week");

        _ = this.Stored<ShoppingListItem>().Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_RecordsThatTheListWasCreated()
    {
        _ = this.Database.Seed(this.Ours);

        await this.HandleAsync("This week");

        this.m_AuditLogic.Verify(a => a.AddAudit(It.IsAny<ShoppingList>()), Times.Once);
    }

    #endregion Methods

}
