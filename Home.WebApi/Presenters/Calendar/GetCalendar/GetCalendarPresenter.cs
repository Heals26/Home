using AutoMapper;
using Home.Application.UseCases.Calendar.GetCalendar;
using Home.Application.UseCases.Calendar.Models;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.Calendar.GetCalendar;
using Home.WebApi.UseCases.Calendar.Models;

namespace Home.WebApi.Presenters.Calendar.GetCalendar;

public class GetCalendarPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IGetCalendarOutputPort
{

    #region Methods

    Task IGetCalendarOutputPort.PresentCalendarAsync(IReadOnlyList<CalendarDay> days, CancellationToken cancellationToken)
        => this.OkAsync(new GetCalendarApiResponse()
        {
            Days = [.. days.Select(d => new CalendarDayDto()
            {
                Date = d.Date,
                Items = [.. d.Items.Select(ToDto)]
            })]
        }, cancellationToken);

    private static CalendarItemDto ToDto(CalendarItem item)
        => new()
        {
            Date = item.Date,
            EndDate = item.EndDate,
            EndsAt = item.EndsAt,
            EndUTC = item.EndUTC,
            ID = item.ID,
            IsAllDay = item.IsAllDay,
            IsDone = item.IsDone,
            IsReadOnly = item.IsReadOnly,
            IsRecurring = item.IsRecurring,
            Kind = item.Kind,
            Location = item.Location,
            OccurrenceDate = item.OccurrenceDate,
            People = [.. item.People],
            PersonUserIDs = [.. item.PersonUserIDs],
            RecipeID = item.RecipeID,
            StartDate = item.StartDate,
            StartsAt = item.StartsAt,
            StartUTC = item.StartUTC,
            Subtitle = item.Subtitle,
            Title = item.Title
        };

    #endregion Methods

}
