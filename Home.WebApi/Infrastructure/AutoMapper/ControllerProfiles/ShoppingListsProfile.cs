using AutoMapper;
using Home.Domain.Entities;
using Home.WebApi.UseCases.ShoppingLists.GetShoppingList;
using Home.WebApi.UseCases.ShoppingLists.GetShoppingLists;

namespace Home.WebApi.Infrastructure.AutoMapper.ControllerProfiles;

public class ShoppingListsProfile : Profile
{

    #region Constructors

    public ShoppingListsProfile()
    {
        _ = this.CreateMap<ShoppingList, GetShoppingListApiResponse>();
        _ = this.CreateMap<IEnumerable<ShoppingList>, GetShoppingListsApiResponse>()
            .ForMember(d => d.ShoppingLists, o => o.MapFrom(s => s));

        _ = this.CreateMap<ShoppingList, GetShoppingListDto>()
            .ForMember(d => d.ItemCount, o => o.MapFrom(s => s.Items.Count));
    }

    #endregion Constructors

}
