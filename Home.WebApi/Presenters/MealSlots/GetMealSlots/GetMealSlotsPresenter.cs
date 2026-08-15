using AutoMapper;
using Home.Application.UseCases.MealSlots.GetMealSlots;
using Home.Domain.Entities;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.MealSlots.GetMealSlots;
using Home.WebApi.UseCases.MealSlots.Models;

namespace Home.WebApi.Presenters.MealSlots.GetMealSlots;

public class GetMealSlotsPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IGetMealSlotsOutputPort
{

    #region Methods

    Task IGetMealSlotsOutputPort.PresentMealSlotsAsync(IEnumerable<MealSlot> mealSlots, CancellationToken cancellationToken)
        => this.OkAsync(new GetMealSlotsApiResponse()
        {
            MealSlots = [.. mealSlots.Select(ms => new MealSlotDto()
            {
                MealSlotID = ms.MealSlotID,
                Name = ms.Name,
                Sequence = ms.Sequence,
                StartsAt = ms.StartsAt
            })]
        }, cancellationToken);

    #endregion Methods

}
