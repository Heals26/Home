using AutoMapper;
using Home.Domain.Entities;
using Home.WebApi.UseCases.Recipes.GetRecipes;
using Home.WebApi.UseCases.Recipes.Models;

namespace Home.WebApi.Infrastructure.AutoMapper.ControllerProfiles;

public class RecipesProfile : Profile
{

    #region Constructors

    public RecipesProfile()
    {
        _ = this.CreateMap<IEnumerable<Recipe>, GetRecipesApiResponse>()
            .ForMember(d => d.Recipes, o => o.MapFrom(s => s));

        _ = this.CreateMap<MealSlot, RecipeMealSlotDto>();

        _ = this.CreateMap<Recipe, GetRecipeDto>()
            .ForMember(d => d.ImageVersion, o => o.MapFrom(s => s.ImageUpdatedOnUTC == null ? null : (long?)s.ImageUpdatedOnUTC.Value.Ticks))
            .ForMember(d => d.MealSlots, o => o.MapFrom(s => s.MealSlots.Select(rms => rms.MealSlot).OrderBy(ms => ms.Sequence)));
    }

    #endregion Constructors

}
