using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.Announcements.CreateAnnouncement;
using Home.Domain.Entities;
using Home.WebApi.Presenters.Announcements.CreateAnnouncement;
using Microsoft.AspNetCore.Mvc;

namespace Home.Application.Tests.UseCases.Announcements.CreateAnnouncement;

/// <summary>
/// Pinning a note to the family board. Deliberately anonymous: the board belongs to the household,
/// not to whoever happened to be at the tablet.
/// </summary>
public class CreateAnnouncementInteractorTests : InteractorTest
{

    #region Fields

    private readonly CreateAnnouncementPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private Task HandleAsync(string content)
        => new CreateAnnouncementInteractor().HandleAsync(
            new CreateAnnouncementInputPort(content),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_PinsTheNoteToTheSignedInHousehold()
    {
        _ = this.Database.Seed(this.Ours);

        await this.HandleAsync("  Bin night  ");

        _ = this.m_Presenter.Result.Should().BeOfType<CreatedResult>();

        var _Stored = this.Stored<Announcement>().Single();

        _ = _Stored.Content.Should().Be("Bin night");
        _ = this.Stored<Announcement>().Count(a => a.Household.HouseholdID == OurHouseholdID).Should().Be(1);
    }

    [Fact]
    public async Task HandleAsync_StampsTheNoteWithTheClockRatherThanReadingItDirectly()
    {
        _ = this.Database.Seed(this.Ours);

        await this.HandleAsync("Bin night");

        _ = this.Stored<Announcement>().Single().CreatedOnUTC.Should().Be(TestServiceFactory.DefaultNow.UtcDateTime);
    }

    #endregion Methods

}
