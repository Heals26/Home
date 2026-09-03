using FluentAssertions;
using Home.Application.Infrastructure.Activities;
using Home.Application.Services.EntityLogic.Activities;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.ActivityRegions.CreateActivityRegion;
using Home.Domain.Entities;
using Home.Domain.Services.Audits;
using Home.WebApi.Presenters.ActivityRegions.CreateActivityRegion;
using Home.WebApi.UseCases.ActivityRegions.CreateActivityRegion;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Home.Application.Tests.UseCases.ActivityRegions.CreateActivityRegion;

/// <summary>
/// Adding a section to a card. <c>ActivityLogic.AddRegion</c> refuses a section belonging to
/// another household by returning null — the guard that stops one family writing under another
/// family's heading — and until 3 Sep the interactor added that null to the card's collection and
/// then read an ID off it, turning the guard into a five hundred.
/// </summary>
public class CreateActivityRegionInteractorTests : InteractorTest
{

    #region Fields

    private readonly Mock<IAuditLogic<Activity>> m_AuditLogic = new();
    private readonly CreateActivityRegionPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private Activity BuildCard(long activityID, Household household)
        => new()
        {
            ActivityID = activityID,
            Household = household,
            Regions = [],
            Title = $"Card {activityID}"
        };

    private Task HandleAsync(long activityID, long cardSectionID)
    {
        var _Services = this.Services(out var _Context);

        return new CreateActivityRegionInteractor().HandleAsync(
            new CreateActivityRegionInputPort(activityID, cardSectionID),
            this.m_Presenter,
            _Services
                .With(this.m_AuditLogic.Object)
                .With<IActivityLogic>(new ActivityLogic(_Context, _Services.Time))
                .Build(),
            CancellationToken.None);
    }

    [Fact]
    public async Task HandleAsync_AddsTheSectionToTheCard()
    {
        _ = this.Database.Seed(
            this.BuildCard(100, this.Ours),
            new CardSection() { CardSectionID = 110, Household = this.Ours, Name = "Details", Sequence = 1 });

        await this.HandleAsync(100, 110);

        _ = this.m_Presenter.PresentedSuccessfully.Should().BeTrue();
        _ = this.m_Presenter.Result.Should().BeOfType<CreatedResult>();
    }

    [Fact]
    public async Task HandleAsync_WhenTheSectionBelongsToAnotherHousehold_RefusesInsteadOfFailing()
    {
        _ = this.Database.Seed(
            this.BuildCard(100, this.Ours),
            new CardSection() { CardSectionID = 110, Household = this.Ours, Name = "Details", Sequence = 1 },
            new CardSection() { CardSectionID = 910, Household = this.Theirs, Name = "Theirs", Sequence = 1 });

        await this.HandleAsync(100, 910);

        ShouldBeNotFound(this.m_Presenter);
    }

    [Fact]
    public async Task HandleAsync_WhenTheSectionDoesNotExistAtAll_RefusesInsteadOfFailing()
    {
        _ = this.Database.Seed(this.BuildCard(100, this.Ours));

        await this.HandleAsync(100, 404);

        ShouldBeNotFound(this.m_Presenter);
    }

    [Fact]
    public async Task HandleAsync_WhenTheCardBelongsToAnotherHousehold_PresentsNotFound()
    {
        _ = this.Database.Seed(
            this.BuildCard(100, this.Ours),
            this.BuildCard(900, this.Theirs),
            new CardSection() { CardSectionID = 110, Household = this.Ours, Name = "Details", Sequence = 1 });

        await this.HandleAsync(900, 110);

        ShouldBeNotFound(this.m_Presenter);
    }

    #endregion Methods

}
