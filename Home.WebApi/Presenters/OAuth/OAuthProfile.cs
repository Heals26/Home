using AutoMapper;
using Home.Application.UseCases.OAuth.CreatePasswordGrant;
using Home.Application.UseCases.OAuth.CreateRefreshGrant;
using Home.Domain.Entities;
using Home.WebApi.UseCases.OAuth.CreatePasswordGrant;
using Home.WebApi.UseCases.OAuth.CreateRefreshGrant;

namespace Home.WebApi.Presenters.OAuth;

public class OAuthProfile : Profile
{

    #region Constructors

    public OAuthProfile()
    {
        _ = this.CreateMap<CreatePasswordGrantApiRequest, CreatePasswordGrantInputPort>();
        _ = this.CreateMap<AuthenticationMetadata, CreatePasswordGrantApiResponse>();

        _ = this.CreateMap<CreateGrantApiRequest, CreateRefreshGrantInputPort>();
        _ = this.CreateMap<AuthenticationMetadata, CreateRefreshGrantApiResponse>();
    }

    #endregion Constructors

}
