using AutoMapper;
using Home.Domain.Entities;
using Home.WebApi.UseCases.ShoppingCarts.GetShoppingCart;
using Home.WebApi.UseCases.ShoppingCarts.GetShoppingCarts;

namespace Home.WebApi.Infrastructure.AutoMapper.ControllerProfiles;

public class ShoppingCartsProfile : Profile
{

    #region Constructors

    public ShoppingCartsProfile()
    {
        _ = this.CreateMap<ShoppingCart, GetShoppingCartApiResponse>();
        _ = this.CreateMap<IEnumerable<ShoppingCart>, GetShoppingCartsApiResponse>()
            .ForMember(d => d.ShoppingCarts, o => o.MapFrom(s => s));

        _ = this.CreateMap<ShoppingCart, GetShoppingCartDto>()
            .ForMember(d => d.ItemCount, o => o.MapFrom(s => s.Items.Count))
            .ForMember(d => d.CreatedBy, o => o.MapFrom(s => s.CreatedBy.GetFullName()));
    }

    #endregion Constructors

}
